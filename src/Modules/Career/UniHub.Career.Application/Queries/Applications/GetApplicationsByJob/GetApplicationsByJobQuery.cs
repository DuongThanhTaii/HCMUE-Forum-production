using UniHub.Career.Application.Commands.Applications.SubmitApplication;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Queries.Applications.GetApplicationsByJob;

/// <summary>
/// Query to retrieve all applications for a specific job posting.
/// Supports filtering by status and pagination.
/// </summary>
public sealed record GetApplicationsByJobQuery(
    Guid JobPostingId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IQuery<ApplicationListResponse>;

/// <summary>
/// Response containing paginated list of applications.
/// </summary>
public sealed record ApplicationListResponse(
    List<ApplicationSummary> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Summary information for an application in list views.
/// </summary>
public sealed record ApplicationSummary(
    Guid Id,
    Guid JobPostingId,
    Guid ApplicantId,
    string Status,
    DateTime SubmittedAt,
    DateTime? LastStatusChangedAt,
    bool HasCoverLetter);
