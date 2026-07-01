namespace UniHub.Forum.Application.Queries.GetBookmarkedPosts;

public sealed record BookmarkedPostDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int PostType { get; init; }
    public int Status { get; init; }
    public Guid AuthorId { get; init; }
    public Guid? CategoryId { get; init; }
    public int VoteScore { get; init; }
    public int CommentCount { get; init; }
    public int ViewCount { get; init; }
    public bool IsPinned { get; init; }
    public DateTime BookmarkedAt { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime? PublishedAt { get; init; }

    public int LikeCount { get; init; }
    public int BookmarkCount { get; init; }
    public int ReplyCount { get; init; }
    public string? CategorySlug { get; init; }
    public string? AuthorAvatar { get; init; }
    public DateTime LastActivity { get; init; }
    public string Preview { get; init; } = string.Empty;
    public bool IsLocked { get; init; }
    public bool IsSolved { get; init; }
    public int? CurrentUserVote { get; init; }
}
