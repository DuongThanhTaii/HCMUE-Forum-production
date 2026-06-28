using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;
using UniHub.Contracts;
using UniHub.Forum.Application.Queries.GetComments;
using UniHub.Forum.Application.Queries.GetPostById;
using UniHub.Forum.Application.Queries.SearchPosts;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/v1/assistant/tools")]
[Produces("application/json")]
[Authorize]
[EnableRateLimiting("ai")]
public sealed class AssistantToolsController : BaseApiController
{
    private readonly ISender _sender;
    private readonly IDocumentSummarizationService _summarizationService;
    private readonly IUniBotService _uniBotService;
    private readonly IContentModerationService _moderationService;

    public AssistantToolsController(
        ISender sender,
        IDocumentSummarizationService summarizationService,
        IUniBotService uniBotService,
        IContentModerationService moderationService)
    {
        _sender = sender;
        _summarizationService = summarizationService;
        _uniBotService = uniBotService;
        _moderationService = moderationService;
    }

    [HttpPost("summarize-post")]
    [ProducesResponseType(typeof(ApiResponse<SummarizePostResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SummarizePost(
        [FromBody] SummarizePostRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PostId == Guid.Empty)
        {
            return BadRequest(ApiResponses.Failure("postId is required."));
        }

        var postResult = await _sender.Send(
            new GetPostByIdQuery(request.PostId, TryGetCurrentUserId()),
            cancellationToken);
        if (postResult.IsFailure || postResult.Value is null)
        {
            return NotFound(ApiResponses.Failure("Post not found."));
        }

        var commentsResult = await _sender.Send(
            new GetCommentsQuery(request.PostId, TryGetCurrentUserId(), 1, 20),
            cancellationToken);
        if (commentsResult.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(commentsResult.Error.Message));
        }

        var summaryInput = BuildSummaryInput(postResult.Value, commentsResult.Value.Comments);
        var summary = await _summarizationService.SummarizeAsync(
            new SummarizationRequest
            {
                Content = summaryInput,
                Title = postResult.Value.Title,
                DocumentType = DocumentType.ForumPost,
                Length = MapLength(request.Length),
                UserId = GetCurrentUserId(),
            },
            cancellationToken);

        if (!summary.IsSuccess)
        {
            return BadRequest(ApiResponses.Failure(summary.ErrorMessage ?? "Failed to summarize post."));
        }

        var response = new SummarizePostResponse(
            request.PostId,
            summary.Summary,
            summary.KeyPoints,
            commentsResult.Value.TotalCount,
            DateTime.UtcNow);
        return Ok(ApiResponses.Success(response));
    }

    [HttpPost("related-posts")]
    [ProducesResponseType(typeof(ApiResponse<RelatedPostsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RelatedPosts(
        [FromBody] RelatedPostsRequest request,
        CancellationToken cancellationToken = default)
    {
        string query;
        Guid? currentPostId = null;

        if (request.PostId.HasValue)
        {
            if (request.PostId.Value == Guid.Empty)
            {
                return BadRequest(ApiResponses.Failure("postId cannot be empty."));
            }

            var postResult = await _sender.Send(
                new GetPostByIdQuery(request.PostId.Value, TryGetCurrentUserId()),
                cancellationToken);
            if (postResult.IsFailure || postResult.Value is null)
            {
                return NotFound(ApiResponses.Failure("Post not found."));
            }

            currentPostId = request.PostId.Value;
            query = $"{postResult.Value.Title} {string.Join(' ', postResult.Value.Tags)}".Trim();
        }
        else
        {
            query = request.Query?.Trim() ?? string.Empty;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(ApiResponses.Failure("query or postId is required."));
        }

        var searchResult = await _sender.Send(
            new SearchPostsQuery(query, null, null, null, 1, Math.Clamp(request.Limit, 1, 20)),
            cancellationToken);

        if (searchResult.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(searchResult.Error.Message));
        }

        var items = searchResult.Value.Posts
            .Where(post => !currentPostId.HasValue || post.Id != currentPostId.Value)
            .Take(Math.Clamp(request.Limit, 1, 20))
            .Select(post => new RelatedPostItem(
                post.Id,
                post.Title,
                post.Slug,
                post.SearchRank,
                BuildRelatedReason(query, post),
                $"/forum/{post.Id}"))
            .ToList();

        return Ok(ApiResponses.Success(new RelatedPostsResponse(query, items)));
    }

    [HttpPost("draft-reply")]
    [ProducesResponseType(typeof(ApiResponse<DraftReplyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DraftReply(
        [FromBody] DraftReplyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PostId == Guid.Empty)
        {
            return BadRequest(ApiResponses.Failure("postId is required."));
        }

        var postResult = await _sender.Send(
            new GetPostByIdQuery(request.PostId, TryGetCurrentUserId()),
            cancellationToken);
        if (postResult.IsFailure || postResult.Value is null)
        {
            return NotFound(ApiResponses.Failure("Post not found."));
        }

        var commentsResult = await _sender.Send(
            new GetCommentsQuery(request.PostId, TryGetCurrentUserId(), 1, 10),
            cancellationToken);
        if (commentsResult.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(commentsResult.Error.Message));
        }

        var prompt = BuildDraftPrompt(postResult.Value, commentsResult.Value.Comments, request.Intent, request.Tone);
        var chatResult = await _uniBotService.ChatAsync(
            new ChatRequest
            {
                Message = prompt,
                UserId = GetCurrentUserId(),
                IncludeHistory = false,
                MaxHistoryMessages = 0,
            },
            cancellationToken);

        if (!chatResult.IsSuccess || string.IsNullOrWhiteSpace(chatResult.Message))
        {
            return BadRequest(ApiResponses.Failure(chatResult.ErrorMessage ?? "Failed to generate draft reply."));
        }

        var response = new DraftReplyResponse(
            request.PostId,
            request.Intent,
            request.Tone,
            chatResult.Message.Trim(),
            DateTime.UtcNow);
        return Ok(ApiResponses.Success(response));
    }

    [HttpPost("suggest-title-tags")]
    [ProducesResponseType(typeof(ApiResponse<SuggestTitleTagsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SuggestTitleTags(
        [FromBody] SuggestTitleTagsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) && string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ApiResponses.Failure("title or content is required."));
        }

        var prompt = $"""
                      You are an assistant for a university forum.
                      Suggest a better post title and tags.

                      Current title:
                      {request.Title}

                      Post content:
                      {request.Content}

                      Return strictly in this format:
                      TITLE: <suggested title>
                      TAGS: <tag1>, <tag2>, <tag3>
                      RATIONALE: <one short sentence>
                      Limit tags to at most {Math.Clamp(request.MaxTags, 1, 8)}.
                      """;

        var result = await _uniBotService.ChatAsync(
            new ChatRequest
            {
                Message = prompt,
                UserId = GetCurrentUserId(),
                IncludeHistory = false,
                MaxHistoryMessages = 0,
            },
            cancellationToken);

        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Message))
        {
            return BadRequest(ApiResponses.Failure(result.ErrorMessage ?? "Failed to suggest title and tags."));
        }

        var (suggestedTitle, tags, rationale) = ParseTitleTagSuggestion(result.Message, request.MaxTags);
        return Ok(ApiResponses.Success(new SuggestTitleTagsResponse(
            suggestedTitle,
            tags,
            rationale,
            DateTime.UtcNow)));
    }

    [HttpPost("rewrite-content")]
    [ProducesResponseType(typeof(ApiResponse<RewriteContentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RewriteContent(
        [FromBody] RewriteContentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return BadRequest(ApiResponses.Failure("content is required."));
        }

        var style = string.IsNullOrWhiteSpace(request.Style) ? "clear" : request.Style.Trim();
        var prompt = $"""
                      Rewrite this forum post content in a {style} style.
                      Keep original intent, improve readability, and keep it concise.
                      Return rewritten content only in markdown.

                      Title: {request.Title}
                      Content:
                      {request.Content}
                      """;

        var result = await _uniBotService.ChatAsync(
            new ChatRequest
            {
                Message = prompt,
                UserId = GetCurrentUserId(),
                IncludeHistory = false,
                MaxHistoryMessages = 0,
            },
            cancellationToken);

        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Message))
        {
            return BadRequest(ApiResponses.Failure(result.ErrorMessage ?? "Failed to rewrite content."));
        }

        return Ok(ApiResponses.Success(new RewriteContentResponse(
            style,
            result.Message.Trim(),
            DateTime.UtcNow)));
    }

    [HttpPost("moderation-hint")]
    [ProducesResponseType(typeof(ApiResponse<ModerationHintResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object?>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ModerationHint(
        [FromBody] ModerationHintRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PostId == Guid.Empty)
        {
            return BadRequest(ApiResponses.Failure("postId is required."));
        }

        var postResult = await _sender.Send(
            new GetPostByIdQuery(request.PostId, TryGetCurrentUserId()),
            cancellationToken);
        if (postResult.IsFailure || postResult.Value is null)
        {
            return NotFound(ApiResponses.Failure("Post not found."));
        }

        var moderation = await _moderationService.ModerateAsync(
            new ModerationRequest
            {
                Content = postResult.Value.Content,
                ContentType = ContentType.ForumPost,
                UserId = postResult.Value.AuthorId,
                Context = $"Forum post: {postResult.Value.Title}",
            },
            cancellationToken);

        if (!moderation.IsSuccess)
        {
            return BadRequest(ApiResponses.Failure(moderation.ErrorMessage ?? "Failed to generate moderation hint."));
        }

        var recommendation = moderation.IsBlocked
            ? "hide"
            : moderation.RequiresReview
                ? "review"
                : "allow";

        var response = new ModerationHintResponse(
            request.PostId,
            moderation.IsSafe,
            moderation.RequiresReview,
            moderation.IsBlocked,
            recommendation,
            moderation.Reason ?? "No specific issue detected",
            moderation.Violations.Select(v => new ModerationViolationItem(
                v.Type.ToString(),
                v.Severity,
                v.Confidence,
                v.Description)).ToList(),
            DateTime.UtcNow);

        return Ok(ApiResponses.Success(response));
    }

    private static SummaryLength MapLength(string? length)
    {
        return length?.Trim().ToLowerInvariant() switch
        {
            "very-short" => SummaryLength.VeryShort,
            "short" => SummaryLength.Short,
            "medium" => SummaryLength.Medium,
            "long" => SummaryLength.Long,
            "detailed" => SummaryLength.Detailed,
            _ => SummaryLength.Medium,
        };
    }

    private static string BuildSummaryInput(PostDetailResult post, IReadOnlyList<CommentItem> comments)
    {
        var commentLines = comments
            .Take(20)
            .Select((comment, index) => $"{index + 1}. {comment.Content}");
        return $"""
                Post title: {post.Title}
                Tags: {string.Join(", ", post.Tags)}
                Content:
                {post.Content}

                Top comments:
                {string.Join(Environment.NewLine, commentLines)}
                """;
    }

    private static string BuildDraftPrompt(
        PostDetailResult post,
        IReadOnlyList<CommentItem> comments,
        string? intent,
        string? tone)
    {
        var normalizedIntent = string.IsNullOrWhiteSpace(intent) ? "answer" : intent.Trim();
        var normalizedTone = string.IsNullOrWhiteSpace(tone) ? "friendly" : tone.Trim();
        var commentSnippet = comments
            .Take(5)
            .Select(comment => $"- {comment.Content}")
            .ToList();

        return $"""
                You are helping a university forum user draft a reply.
                Intent: {normalizedIntent}
                Tone: {normalizedTone}

                Post title: {post.Title}
                Post content:
                {post.Content}

                Existing comments:
                {string.Join(Environment.NewLine, commentSnippet)}

                Generate a concise Markdown reply draft with:
                - direct answer first
                - 1-3 practical suggestions
                - a friendly closing
                """;
    }

    private static string BuildRelatedReason(string query, PostSearchResult post)
    {
        return $"Matched '{query}' with rank {post.SearchRank:F3}, tags: {string.Join(", ", post.Tags)}";
    }

    private static (string title, IReadOnlyList<string> tags, string rationale) ParseTitleTagSuggestion(
        string raw,
        int maxTags)
    {
        var lines = raw.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string title = string.Empty;
        string rationale = string.Empty;
        var tags = new List<string>();

        foreach (var line in lines)
        {
            if (line.StartsWith("TITLE:", StringComparison.OrdinalIgnoreCase))
            {
                title = line["TITLE:".Length..].Trim();
            }
            else if (line.StartsWith("TAGS:", StringComparison.OrdinalIgnoreCase))
            {
                var part = line["TAGS:".Length..].Trim();
                tags = part
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(Math.Clamp(maxTags, 1, 8))
                    .ToList();
            }
            else if (line.StartsWith("RATIONALE:", StringComparison.OrdinalIgnoreCase))
            {
                rationale = line["RATIONALE:".Length..].Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            title = lines.FirstOrDefault()?.Trim() ?? "Improved forum post title";
        }

        if (tags.Count == 0)
        {
            tags = ["discussion", "campus"];
        }

        if (string.IsNullOrWhiteSpace(rationale))
        {
            rationale = "Improved clarity and discoverability for forum readers.";
        }

        return (title, tags, rationale);
    }
}

public sealed record SummarizePostRequest(Guid PostId, string? Length = null);

public sealed record SummarizePostResponse(
    Guid PostId,
    string Summary,
    IReadOnlyList<string> KeyPoints,
    int CommentCount,
    DateTime GeneratedAt);

public sealed record RelatedPostsRequest(Guid? PostId = null, string? Query = null, int Limit = 5);

public sealed record RelatedPostsResponse(
    string Query,
    IReadOnlyList<RelatedPostItem> Items);

public sealed record RelatedPostItem(
    Guid Id,
    string Title,
    string Slug,
    double SearchRank,
    string Reason,
    string CitationUrl);

public sealed record DraftReplyRequest(Guid PostId, string Intent = "answer", string Tone = "friendly");

public sealed record DraftReplyResponse(
    Guid PostId,
    string Intent,
    string Tone,
    string DraftMarkdown,
    DateTime GeneratedAt);

public sealed record SuggestTitleTagsRequest(
    string Title,
    string Content,
    int MaxTags = 5);

public sealed record SuggestTitleTagsResponse(
    string SuggestedTitle,
    IReadOnlyList<string> SuggestedTags,
    string Rationale,
    DateTime GeneratedAt);

public sealed record RewriteContentRequest(
    string Title,
    string Content,
    string Style = "clear");

public sealed record RewriteContentResponse(
    string Style,
    string RewrittenContent,
    DateTime GeneratedAt);

public sealed record ModerationHintRequest(Guid PostId);

public sealed record ModerationHintResponse(
    Guid PostId,
    bool IsSafe,
    bool RequiresReview,
    bool IsBlocked,
    string Recommendation,
    string Reason,
    IReadOnlyList<ModerationViolationItem> Violations,
    DateTime GeneratedAt);

public sealed record ModerationViolationItem(
    string Type,
    double Severity,
    double Confidence,
    string Description);
