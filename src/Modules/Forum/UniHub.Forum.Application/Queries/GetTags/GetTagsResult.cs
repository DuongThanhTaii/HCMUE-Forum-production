namespace UniHub.Forum.Application.Queries.GetTags;

public sealed record GetTagsResult
{
    public IEnumerable<TagDto> Tags { get; init; } = Enumerable.Empty<TagDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
