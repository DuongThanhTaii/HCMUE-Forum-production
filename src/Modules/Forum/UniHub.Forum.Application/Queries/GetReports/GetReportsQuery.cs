using UniHub.SharedKernel.CQRS;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Queries.GetReports;

public sealed record GetReportsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    ReportStatus? Status = null,
    ReportResolutionDecision? ResolutionDecision = null,
    IReadOnlyList<Guid>? CategoryIds = null) : IQuery<GetReportsResult>;
