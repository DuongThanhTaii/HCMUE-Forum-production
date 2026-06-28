using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.SavedJobs.GetSavedJobs;

/// <summary>
/// Query to get user's saved jobs with pagination.
/// </summary>
public sealed record GetSavedJobsQuery(
    Guid UserId,
    int Page = 1,
    int PageSize = 10
) : IRequest<Result<SavedJobsResponse>>;

/// <summary>
/// Response containing paginated list of saved jobs.
/// </summary>
public sealed record SavedJobsResponse(
    List<SavedJobDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

/// <summary>
/// DTO representing a saved job with job posting details.
/// </summary>
public sealed record SavedJobDto(
    Guid JobPostingId,
    string Title,
    Guid CompanyId,
    string CompanyName,
    string JobType,
    string ExperienceLevel,
    string Status,
    LocationDto Location,
    SalaryDto? Salary,
    DateTime? PublishedAt,
    DateTime? Deadline,
    int ViewCount,
    int ApplicationCount,
    DateTime SavedAt
);

/// <summary>
/// DTO for location information.
/// </summary>
public sealed record LocationDto(
    string City,
    string? District,
    bool IsRemote
);

/// <summary>
/// DTO for salary information.
/// </summary>
public sealed record SalaryDto(
    decimal MinAmount,
    decimal MaxAmount,
    string Currency,
    string Period
);
