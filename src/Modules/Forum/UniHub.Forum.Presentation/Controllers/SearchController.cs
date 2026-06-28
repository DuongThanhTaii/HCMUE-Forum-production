using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Forum.Application.Queries.SearchPosts;
using UniHub.Forum.Presentation.DTOs.Posts;

namespace UniHub.Forum.Presentation.Controllers;

[ApiController]
[Route("api/v1/search")]
[Produces("application/json")]
public class SearchController : ControllerBase
{
    private readonly ISender _sender;

    public SearchController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Search posts by title, content, and tags
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SearchPostsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SearchPosts(
        [FromQuery] string q,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] int? postType = null,
        [FromQuery] string? tags = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(ApiResponses.Failure("Search term is required"));
        }

        var tagList = string.IsNullOrWhiteSpace(tags)
            ? null
            : tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var query = new SearchPostsQuery(
            q,
            categoryId,
            postType,
            tagList,
            pageNumber,
            pageSize);

        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new SearchPostsResponse
        {
            Posts = result.Value.Posts.Select(p => new SearchPostItem
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                Slug = p.Slug,
                Type = p.Type,
                AuthorId = p.AuthorId,
                CategoryId = p.CategoryId,
                Tags = p.Tags,
                VoteScore = p.VoteScore,
                CommentCount = p.CommentCount,
                IsPinned = p.IsPinned,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                SearchRank = p.SearchRank
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
}

public sealed record SearchPostsResponse
{
    public IReadOnlyList<SearchPostItem> Posts { get; init; } = Array.Empty<SearchPostItem>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}

public sealed record SearchPostItem
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int Type { get; init; }
    public Guid AuthorId { get; init; }
    public Guid? CategoryId { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public int VoteScore { get; init; }
    public int CommentCount { get; init; }
    public bool IsPinned { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public double SearchRank { get; init; }
}
