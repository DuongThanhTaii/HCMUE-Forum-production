using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Forum.Application.Commands.BookmarkPost;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Application.Commands.DeletePost;
using UniHub.Forum.Application.Commands.PinPost;
using UniHub.Forum.Application.Commands.PublishPost;
using UniHub.Forum.Application.Commands.ReportPost;
using UniHub.Forum.Application.Commands.UnbookmarkPost;
using UniHub.Forum.Application.Commands.UpdatePost;
using UniHub.Forum.Application.Commands.VotePost;
using UniHub.Forum.Application.Queries.GetBookmarkedPosts;
using UniHub.Forum.Application.Queries.GetComments;
using UniHub.Forum.Application.Queries.GetPostById;
using UniHub.Forum.Application.Queries.GetPosts;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;
using UniHub.Forum.Presentation.DTOs.Comments;
using UniHub.Forum.Presentation.DTOs.Posts;
using UniHub.Forum.Presentation.DTOs.Reports;
using UniHub.Forum.Presentation.DTOs.Votes;
using UniHub.Forum.Presentation.Services;

namespace UniHub.Forum.Presentation.Controllers;

[Route("api/v1/posts")]
[Produces("application/json")]
public class PostsController : BaseApiController
{
    private readonly ISender _sender;
    private readonly IForumAttachmentStorageService _attachmentStorageService;

    public PostsController(
        ISender sender,
        IForumAttachmentStorageService attachmentStorageService)
    {
        _sender = sender;
        _attachmentStorageService = attachmentStorageService;
    }

    /// <summary>
    /// Get a paginated list of posts with optional filtering
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PostListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPosts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] Guid? threadChannelId = null,
        [FromQuery] int? type = null,
        [FromQuery] int? status = null,
        [FromQuery] int sortBy = 0,
        CancellationToken cancellationToken = default)
    {
        var effectiveStatus = status ?? (int)PostStatus.Published;
        var query = new GetPostsQuery(pageNumber, pageSize, categoryId, threadChannelId, type, effectiveStatus, sortBy);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new PostListResponse
        {
            Posts = result.Value.Posts.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                Slug = p.Slug,
                Type = p.Type,
                Status = p.Status,
                AuthorId = p.AuthorId,
                CategoryId = p.CategoryId,
                ThreadChannelId = p.ThreadChannelId,
                ThreadChannelCode = p.ThreadChannelCode,
                ThreadChannelName = p.ThreadChannelName,
                CategoryName = p.CategoryName,
                AuthorName = p.AuthorName,
                Tags = p.Tags,
                VoteScore = p.VoteScore,
                CommentCount = p.CommentCount,
                IsPinned = p.IsPinned,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                PublishedAt = p.PublishedAt
            }).ToList(),
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            TotalPages = result.Value.TotalPages,
            HasPreviousPage = result.Value.HasPreviousPage,
            HasNextPage = result.Value.HasNextPage
        };

        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Get bookmarked posts for current user
    /// </summary>
    [HttpGet("bookmarks")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<PostListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetBookmarkedPosts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetBookmarkedPostsQuery(GetCurrentUserId(), pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new PostListResponse
        {
            Posts = result.Value.Posts.Select(p => new PostResponse
            {
                Id = p.Id,
                Title = p.Title,
                Content = string.Empty,
                Slug = p.Slug,
                Type = p.PostType,
                Status = p.Status,
                AuthorId = p.AuthorId,
                CategoryId = p.CategoryId,
                VoteScore = p.VoteScore,
                CommentCount = p.CommentCount,
                IsBookmarked = true,
                IsPinned = p.IsPinned,
                CreatedAt = p.CreatedAt,
                PublishedAt = p.PublishedAt
            }).ToList(),
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            TotalPages = result.Value.TotalPages,
            HasPreviousPage = result.Value.HasPreviousPage,
            HasNextPage = result.Value.HasNextPage
        };

        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Get a post by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPostById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPostByIdQuery(id, TryGetCurrentUserId());
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure || result.Value == null)
        {
            return NotFound(ApiResponses.Failure("Post not found"));
        }

        var response = new PostResponse
        {
            Id = result.Value.Id,
            Title = result.Value.Title,
            Content = result.Value.Content,
            Slug = result.Value.Slug,
            Type = result.Value.Type,
            Status = result.Value.Status,
            AuthorId = result.Value.AuthorId,
            CategoryId = result.Value.CategoryId,
            ThreadChannelId = result.Value.ThreadChannelId,
            ThreadChannelCode = result.Value.ThreadChannelCode,
            ThreadChannelName = result.Value.ThreadChannelName,
            CategoryName = result.Value.CategoryName,
            AuthorName = result.Value.AuthorName,
            Tags = result.Value.Tags,
            VoteScore = result.Value.VoteScore,
            CommentCount = result.Value.CommentCount,
            IsBookmarked = result.Value.IsBookmarkedByCurrentUser,
            IsPinned = result.Value.IsPinned,
            CreatedAt = result.Value.CreatedAt,
            UpdatedAt = result.Value.UpdatedAt,
            PublishedAt = result.Value.PublishedAt
        };

        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Create a new post
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePost(
        [FromBody] CreatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        var authorId = GetCurrentUserId();

        var command = new CreatePostCommand(
            request.Title,
            request.Content,
            request.Type,
            authorId,
            request.CategoryId,
            request.ThreadChannelId,
            request.Tags);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var payload = ApiResponses.Success((object)new { postId = result.Value }, "Post created successfully");
        return CreatedAtAction(
            nameof(GetPostById),
            new { id = result.Value },
            payload);
    }

    /// <summary>
    /// Update a post
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePost(
        Guid id,
        [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new UpdatePostCommand(
            id,
            request.Title ?? string.Empty,
            request.Content ?? string.Empty,
            request.CategoryId,
            request.ThreadChannelId,
            request.Tags,
            userId);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Post updated successfully"));
    }

    /// <summary>
    /// Delete a post
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePost(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new DeletePostCommand(id, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Post deleted successfully"));
    }

    /// <summary>
    /// Publish a post
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PublishPost(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var actor = User.IsInRole("Admin")
            ? PostPublishActor.Admin
            : User.IsInRole("Moderator")
                ? PostPublishActor.Moderator
                : PostPublishActor.Author;

        var command = new PublishPostCommand(id, userId, actor);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Post published successfully"));
    }

    /// <summary>
    /// Pin or unpin a post
    /// </summary>
    [HttpPost("{id:guid}/pin")]
    [RequirePermission("forum.posts.update")]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PinPost(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new PinPostCommand(id, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Post pin state updated successfully"));
    }

    /// <summary>
    /// Vote on a post (upvote or downvote)
    /// </summary>
    [HttpPost("{id:guid}/vote")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VotePost(
        Guid id,
        [FromBody] VoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var voteType = request.VoteType == 1 ? VoteType.Upvote : VoteType.Downvote;
        var command = new VotePostCommand(id, userId, voteType);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Post voted successfully"));
    }

    /// <summary>
    /// Get comments for a post
    /// </summary>
    [HttpGet("{id:guid}/comments")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<CommentListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPostComments(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCommentsQuery(id, TryGetCurrentUserId(), pageNumber, pageSize);
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new CommentListResponse
        {
            Comments = result.Value.Comments.Select(c => new CommentResponse
            {
                Id = c.Id,
                PostId = c.PostId,
                AuthorId = c.AuthorId,
                AuthorName = string.IsNullOrWhiteSpace(c.AuthorName) ? null : c.AuthorName,
                Content = c.Content,
                ParentCommentId = c.ParentCommentId,
                VoteScore = c.VoteScore,
                CurrentUserVote = c.CurrentUserVote,
                IsAcceptedAnswer = c.IsAcceptedAnswer,
                IsPinned = c.IsPinned,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            }).ToList(),
            TotalCount = result.Value.TotalCount,
            PageNumber = result.Value.PageNumber,
            PageSize = result.Value.PageSize,
            TotalPages = result.Value.TotalPages,
            HasPreviousPage = result.Value.HasPreviousPage,
            HasNextPage = result.Value.HasNextPage
        };

        return Ok(ApiResponses.Success(response));
    }

    /// <summary>
    /// Bookmark a post
    /// </summary>
    [HttpPost("{id:guid}/bookmark")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BookmarkPost(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new BookmarkPostCommand(id, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Post bookmarked successfully"));
    }

    /// <summary>
    /// Remove bookmark from a post
    /// </summary>
    [HttpDelete("{id:guid}/bookmark")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnbookmarkPost(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new UnbookmarkPostCommand(id, userId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Post unbookmarked successfully"));
    }

    /// <summary>
    /// Report a post
    /// </summary>
    [HttpPost("{id:guid}/report")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReportPost(
        Guid id,
        [FromBody] ReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();

        var command = new ReportPostCommand(
            id,
            userId,
            (UniHub.Forum.Domain.Reports.ReportReason)request.Reason,
            request.Description);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Created(string.Empty, ApiResponses.Success((object)new { reportId = result.Value }, "Post reported successfully"));
    }

    /// <summary>
    /// Upload attachments for forum posts/comments and return Cloudinary URLs.
    /// </summary>
    [HttpPost("attachments/upload")]
    [Authorize]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<UploadForumAttachmentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadAttachments(
        [FromForm] UploadForumAttachmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Files.Count == 0)
        {
            return BadRequest(ApiResponses.Failure("At least one file is required."));
        }

        var validFiles = request.Files.Where(f => f.Length > 0).ToList();
        if (validFiles.Count == 0)
        {
            return BadRequest(ApiResponses.Failure("All uploaded files are empty."));
        }

        var userId = GetCurrentUserId();
        var urls = await _attachmentStorageService.UploadAsync(validFiles, userId, cancellationToken);
        var response = new UploadForumAttachmentsResponse(urls);

        return Ok(ApiResponses.Success(response, "Forum attachments uploaded successfully"));
    }
}
