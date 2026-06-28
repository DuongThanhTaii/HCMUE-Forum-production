using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.ResolveModerationReport;
using UniHub.Forum.Application.Queries.GetPosts;
using UniHub.Forum.Application.Queries.GetReports;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Reports;
using UniHub.Forum.Presentation.DTOs.Moderation;
using UniHub.Forum.Presentation.DTOs.Posts;

namespace UniHub.Forum.Presentation.Controllers;

[Route("api/v1/mod")]
[Produces("application/json")]
[RequirePermission("forum.reports.review")]
public sealed class ModerationController : BaseApiController
{
    private readonly ISender _sender;
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ICategoryRepository _categoryRepository;

    public ModerationController(
        ISender sender,
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        ICategoryRepository categoryRepository)
    {
        _sender = sender;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _categoryRepository = categoryRepository;
    }

    [HttpGet("reports")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReports(
        [FromQuery] string? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var mapped = status switch
        {
            null or "" or "pending" => ReportStatus.Pending,
            "resolved_keep" => ReportStatus.Resolved,
            "resolved_remove" => ReportStatus.Resolved,
            _ => (ReportStatus?)null
        };
        var resolutionDecision = status switch
        {
            "resolved_keep" => ReportResolutionDecision.Keep,
            "resolved_remove" => ReportResolutionDecision.Remove,
            _ => (ReportResolutionDecision?)null
        };

        if (!string.IsNullOrWhiteSpace(status) && mapped is null)
        {
            return BadRequest(ApiResponses.Failure("Invalid status filter."));
        }

        var moderatorId = GetCurrentUserId();
        var scopedCategoryIds = await ResolveScopedCategoryIdsAsync(
            moderatorId,
            isAdmin: User.IsInRole("Admin"),
            cancellationToken);

        var result = await _sender.Send(new GetReportsQuery(pageNumber, pageSize, mapped, resolutionDecision, scopedCategoryIds), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var responses = new List<ModerationReportResponse>(result.Value.Reports.Count);
        foreach (var report in result.Value.Reports)
        {
            var snapshot = await GetSnapshotAsync(report.ReportedItemType, report.ReportedItemId, cancellationToken);
            responses.Add(new ModerationReportResponse
            {
                Id = report.Id,
                ReportedItemId = report.ReportedItemId,
                ReportedItemType = (int)report.ReportedItemType,
                ReporterId = report.ReporterId,
                Reason = (int)report.Reason,
                Description = report.Description,
                Status = (int)report.Status,
                CreatedAt = report.CreatedAt,
                ReviewedAt = report.ReviewedAt,
                ReviewedBy = report.ReviewedBy,
                ResolutionDecision = report.ResolutionDecision switch
                {
                    ReportResolutionDecision.Keep => "keep",
                    ReportResolutionDecision.Remove => "remove",
                    _ => null
                },
                TitlePreview = snapshot.Title,
                ContentPreview = snapshot.Content,
                IsTargetDeleted = snapshot.IsDeleted
            });
        }

        var payload = new
        {
            reports = responses,
            result.Value.TotalCount,
            result.Value.PageNumber,
            result.Value.PageSize,
            result.Value.TotalPages,
            result.Value.HasPreviousPage,
            result.Value.HasNextPage
        };

        return Ok(ApiResponses.Success(payload));
    }

    [HttpPost("reports/{id:int}/resolve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ResolveReport(
        int id,
        [FromBody] ResolveModerationReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var reviewerId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        var scopedCategoryIds = await ResolveScopedCategoryIdsAsync(reviewerId, isAdmin, cancellationToken);

        var result = await _sender.Send(
            new ResolveModerationReportCommand(
                id,
                reviewerId,
                request.Action?.Trim().ToLowerInvariant() ?? string.Empty,
                isAdmin,
                scopedCategoryIds),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.Code switch
            {
                "Report.NotFound" => NotFound(ApiResponses.Failure(result.Error.Message)),
                "Report.AlreadyResolved" => Conflict(ApiResponses.Failure(result.Error.Message)),
                "Moderation.Forbidden" => StatusCode(StatusCodes.Status403Forbidden, ApiResponses.Failure(result.Error.Message)),
                _ => BadRequest(ApiResponses.Failure(result.Error.Message))
            };
        }

        return Ok(ApiResponses.Success("Report resolved successfully."));
    }

    [HttpGet("posts")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPosts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var moderatorId = GetCurrentUserId();
        var scopedCategoryIds = await ResolveScopedCategoryIdsAsync(
            moderatorId,
            isAdmin: User.IsInRole("Admin"),
            cancellationToken);

        var result = await _sender.Send(
            new GetPostsQuery(
                PageNumber: pageNumber,
                PageSize: pageSize,
                CategoryId: null,
                ThreadChannelId: null,
                Type: null,
                Status: (int)PostStatus.Draft,
                SortBy: 0,
                CategoryIds: scopedCategoryIds),
            cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var payload = new
        {
            posts = result.Value.Posts.Select(p => new PostResponse
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
                UpdatedAt = p.UpdatedAt
            }).ToList(),
            result.Value.TotalCount,
            result.Value.PageNumber,
            result.Value.PageSize,
            result.Value.TotalPages,
            result.Value.HasPreviousPage,
            result.Value.HasNextPage
        };

        return Ok(ApiResponses.Success(payload));
    }

    private async Task<(string? Title, string? Content, bool IsDeleted)> GetSnapshotAsync(
        ReportedItemType type,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        if (type == ReportedItemType.Post)
        {
            var post = await _postRepository.GetByIdAsync(new PostId(itemId), cancellationToken);
            if (post is null)
            {
                return (null, null, true);
            }

            var content = post.Content.Value;
            return (TrimPreview(post.Title.Value, 120), TrimPreview(content, 240), post.Status == PostStatus.Deleted);
        }

        var comment = await _commentRepository.GetByIdAsync(new CommentId(itemId), cancellationToken);
        if (comment is null)
        {
            return (null, null, true);
        }

        return (null, TrimPreview(comment.Content.Value, 240), comment.IsDeleted);
    }

    private static string? TrimPreview(string? value, int maxLen)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();
        return text.Length <= maxLen ? text : text[..maxLen];
    }

    private async Task<IReadOnlyList<Guid>?> ResolveScopedCategoryIdsAsync(
        Guid moderatorId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        if (isAdmin)
        {
            return null;
        }

        var allCategories = await _categoryRepository.GetAllAsync(cancellationToken);
        var assignedCategoryIds = allCategories
            .Where(c => c.ModeratorIds.Contains(moderatorId))
            .Select(c => c.Id.Value)
            .ToList();

        // Hotfix: if moderator has no explicit category assignment yet,
        // do not hard-filter to empty set, otherwise moderation inbox appears blank.
        // TODO: tighten this once category-moderator assignment flow is finalized.
        return assignedCategoryIds.Count == 0 ? null : assignedCategoryIds;
    }
}
