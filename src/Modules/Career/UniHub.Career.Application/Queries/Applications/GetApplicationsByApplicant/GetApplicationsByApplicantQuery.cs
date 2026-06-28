using UniHub.Career.Application.Commands.Applications.SubmitApplication;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Queries.Applications.GetApplicationsByApplicant;

/// <summary>
/// Query to retrieve all applications submitted by a specific applicant.
/// Supports filtering by status and pagination.
/// </summary>
public sealed record GetApplicationsByApplicantQuery(
    Guid ApplicantId,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IQuery<ApplicationsByApplicantResponse>;

/// <summary>
/// Response containing paginated list of applicant's applications.
/// </summary>
public sealed record ApplicationsByApplicantResponse(
    List<ApplicantApplicationSummary> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Summary for applicant's application including job posting title.
/// </summary>
public sealed record ApplicantApplicationSummary(
    Guid Id,
    Guid JobPostingId,
    string JobPostingTitle,
    string CompanyName,
    string Status,
    DateTime SubmittedAt,
    DateTime? LastStatusChangedAt,
    bool HasCoverLetter);
