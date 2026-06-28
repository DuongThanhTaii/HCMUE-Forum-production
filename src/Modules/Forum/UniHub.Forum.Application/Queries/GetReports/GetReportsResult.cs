namespace UniHub.Forum.Application.Queries.GetReports;

public sealed class GetReportsResult
{
    public IReadOnlyList<ReportDto> Reports { get; init; } = new List<ReportDto>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public bool HasPreviousPage { get; init; }
    public bool HasNextPage { get; init; }
}
