namespace UniHub.Forum.Application.Queries.GetPostById;

/// <summary>
/// Detailed information about a post
/// </summary>
public sealed record PostDetailResult
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int Type { get; init; }
    public int Status { get; init; }
    public Guid AuthorId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? ThreadChannelId { get; init; }
    public string? ThreadChannelCode { get; init; }
    public string? ThreadChannelName { get; init; }
    public string? CategoryName { get; init; }
    public string? AuthorName { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public int VoteScore { get; init; }
    public int CommentCount { get; init; }
    public bool IsBookmarkedByCurrentUser { get; init; }
    public bool IsPinned { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }
}
