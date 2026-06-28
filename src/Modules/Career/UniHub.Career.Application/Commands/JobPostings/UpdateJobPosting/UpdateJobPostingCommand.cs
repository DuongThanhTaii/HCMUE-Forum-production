using UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.JobPostings.UpdateJobPosting;

/// <summary>
/// Command to update an existing job posting.
/// Only allowed when status is Draft or Paused.
/// </summary>
public sealed record UpdateJobPostingCommand(
    Guid JobPostingId,
    string Title,
    string Description,
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
