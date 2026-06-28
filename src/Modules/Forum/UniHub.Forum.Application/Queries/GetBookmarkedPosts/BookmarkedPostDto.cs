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
    public DateTime? PublishedAt { get; init; }
}
