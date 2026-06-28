using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Queries.JobPostings.GetJobPostings;

/// <summary>
/// Query to get a paginated list of job postings with optional filters.
/// </summary>
public sealed record GetJobPostingsQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? CompanyId = null,
    JobType? JobType = null,
    ExperienceLevel? ExperienceLevel = null,
    JobPostingStatus? Status = null,
    string? City = null,
    bool? IsRemote = null,
    string? SearchTerm = null) : IQuery<JobPostingListResponse>;

/// <summary>
/// Response containing paginated job postings.
/// </summary>
public sealed record JobPostingListResponse(
    List<JobPostingSummary> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

/// <summary>
/// Summary DTO for job posting in list views.
/// </summary>
public sealed record JobPostingSummary(
    Guid JobPostingId,
    string Title,
    string Description,
    Guid CompanyId,
    string? CompanyName,
    string? CompanyLogoUrl,
    string JobType,
    string ExperienceLevel,
    string Status,
    string City,
    bool IsRemote,
    SalaryInfo? Salary,
    DateTime? Deadline,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    int ViewCount,
    int ApplicationCount);

public sealed record SalaryInfo(
    decimal MinAmount,
    decimal MaxAmount,
    string Currency,
    string Period);
