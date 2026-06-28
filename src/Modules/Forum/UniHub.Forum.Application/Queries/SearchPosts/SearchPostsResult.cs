namespace UniHub.Forum.Application.Queries.SearchPosts;

/// <summary>
/// Result of a post search query with pagination
/// </summary>
public sealed record SearchPostsResult
{
    public IReadOnlyList<PostSearchResult> Posts { get; init; } = Array.Empty<PostSearchResult>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
