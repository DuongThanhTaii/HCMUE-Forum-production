namespace UniHub.Forum.Application.Queries.SearchPosts;

/// <summary>
/// Represents a post in search results
/// </summary>
public sealed record PostSearchResult
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
    
    /// <summary>
    /// Search relevance rank (higher is more relevant)
    /// Calculated by PostgreSQL full-text search
    /// </summary>
    public double SearchRank { get; init; }
}
