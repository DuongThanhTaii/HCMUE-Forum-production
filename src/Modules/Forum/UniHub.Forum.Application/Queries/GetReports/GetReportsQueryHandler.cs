using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Queries.GetReports;

public sealed class GetReportsQueryHandler : IQueryHandler<GetReportsQuery, GetReportsResult>
{
    private readonly IReportRepository _reportRepository;

    public GetReportsQueryHandler(IReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<Result<GetReportsResult>> Handle(
        GetReportsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.PageNumber <= 0)
        {
            return Result.Failure<GetReportsResult>(ReportErrors.InvalidPageNumber);
        }

        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<GetReportsResult>(ReportErrors.InvalidPageSize);
        }

        var (reports, totalCount) = await _reportRepository.GetReportsAsync(
            request.PageNumber,
            request.PageSize,
            request.Status,
            request.ResolutionDecision,
            request.CategoryIds,
            cancellationToken);

        var reportDtos = reports.Select(r => new ReportDto
        {
            Id = r.Id.Value,
            ReportedItemId = r.ReportedItemId,
            ReportedItemType = r.ReportedItemType,
            ReporterId = r.ReporterId,
            Reason = r.Reason,
            Description = r.Description,
            Status = r.Status,
            ResolutionDecision = r.ResolutionDecision,
            CreatedAt = r.CreatedAt,
            ReviewedAt = r.ReviewedAt,
            ReviewedBy = r.ReviewedBy
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var result = new GetReportsResult
        {
            Reports = reportDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            HasPreviousPage = request.PageNumber > 1,
            HasNextPage = request.PageNumber < totalPages
        };

        return Result.Success(result);
    }
}
