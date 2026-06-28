using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;

/// <summary>
/// Command to create a new job posting in Draft status.
/// </summary>
public sealed record CreateJobPostingCommand(
    string Title,
    string Description,
    Guid CompanyId,
    Guid PostedBy,
    JobType JobType,
    ExperienceLevel ExperienceLevel,
    string City,
    string? District = null,
    string? Address = null,
    bool IsRemote = false,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    string? SalaryCurrency = null,
    string? SalaryPeriod = null,
    DateTime? Deadline = null) : ICommand<JobPostingResponse>;
