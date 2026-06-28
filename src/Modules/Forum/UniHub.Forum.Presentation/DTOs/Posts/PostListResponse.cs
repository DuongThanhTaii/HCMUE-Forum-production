namespace UniHub.Forum.Presentation.DTOs.Posts;

/// <summary>
/// Paginated response for post lists
/// </summary>
public sealed record PostListResponse
{
    public IReadOnlyList<PostResponse> Posts { get; init; } = Array.Empty<PostResponse>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
