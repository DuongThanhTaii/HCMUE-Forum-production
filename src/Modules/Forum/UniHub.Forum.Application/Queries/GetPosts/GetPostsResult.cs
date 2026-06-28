namespace UniHub.Forum.Application.Queries.GetPosts;

/// <summary>
/// Result of get posts query with pagination
/// </summary>
public sealed record GetPostsResult
{
    public IReadOnlyList<PostItem> Posts { get; init; } = Array.Empty<PostItem>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
