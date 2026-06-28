namespace UniHub.Forum.Presentation.DTOs.Comments;

/// <summary>
/// Paginated response for comment lists
/// </summary>
public sealed record CommentListResponse
{
    public IReadOnlyList<CommentResponse> Comments { get; init; } = Array.Empty<CommentResponse>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
