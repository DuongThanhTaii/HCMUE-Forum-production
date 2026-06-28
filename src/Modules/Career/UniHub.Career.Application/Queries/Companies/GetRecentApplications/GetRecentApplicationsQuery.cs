using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Companies.GetRecentApplications;

/// <summary>
/// Query to get recent applications across all company job postings.
/// </summary>
public sealed record GetRecentApplicationsQuery(
    Guid CompanyId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<RecentApplicationsResponse>>;

/// <summary>
/// Response containing recent applications for company dashboard.
/// </summary>
public sealed record RecentApplicationsResponse(
    List<ApplicationSummaryDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// Application summary for company dashboard.
/// </summary>
public sealed record ApplicationSummaryDto(
    Guid ApplicationId,
    Guid JobPostingId,
    string JobTitle,
    Guid ApplicantId,
    string ApplicantName,
    string Status,
    DateTime SubmittedAt,
    DateTime? LastStatusChangedAt,
    bool HasCoverLetter
);
