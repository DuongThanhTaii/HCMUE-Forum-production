namespace UniHub.Forum.Application.Queries.GetPosts;

/// <summary>
/// Represents a post in the list
/// </summary>
public sealed record PostItem
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
    /// <summary>Resolved category title for API consumers.</summary>
    public string? CategoryName { get; init; }
    /// <summary>Resolved author display name.</summary>
    public string? AuthorName { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
    public int VoteScore { get; init; }
    public int CommentCount { get; init; }
    public bool IsPinned { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }

    // New required fields for modern Thread Card
    public int ViewCount { get; init; }
    public bool IsLocked { get; init; }
    public bool IsSolved { get; init; }
    public string? AuthorAvatar { get; init; }
    public string Preview { get; init; } = string.Empty;
    public string? CategorySlug { get; init; }
    public DateTime LastActivity { get; init; }
    public int ReplyCount { get; init; }
    public int LikeCount { get; init; }
    public int BookmarkCount { get; init; }
}
