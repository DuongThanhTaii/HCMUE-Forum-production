using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;

/// <summary>
/// Response DTO for job posting operations.
/// </summary>
public sealed record JobPostingResponse(
    Guid JobPostingId,
    string Title,
    string Description,
    Guid CompanyId,
    Guid PostedBy,
    string JobType,
    string ExperienceLevel,
    string Status,
    SalaryInfo? Salary,
    LocationInfo Location,
    DateTime? Deadline,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? PublishedAt,
    int ViewCount,
    int ApplicationCount,
    List<string> Tags);

public sealed record SalaryInfo(
    decimal MinAmount,
    decimal MaxAmount,
    string Currency,
    string Period);

public sealed record LocationInfo(
    string City,
    string? District,
    string? Address,
    bool IsRemote);
