namespace UniHub.Forum.Application.Queries.GetComments;

/// <summary>
/// Result of get comments query with pagination
/// </summary>
public sealed record GetCommentsResult
{
    public IReadOnlyList<CommentItem> Comments { get; init; } = Array.Empty<CommentItem>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
