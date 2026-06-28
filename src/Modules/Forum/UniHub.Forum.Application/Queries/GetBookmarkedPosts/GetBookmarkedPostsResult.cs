namespace UniHub.Forum.Application.Queries.GetBookmarkedPosts;

public sealed record GetBookmarkedPostsResult
{
    public IEnumerable<BookmarkedPostDto> Posts { get; init; } = Enumerable.Empty<BookmarkedPostDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
