using UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.JobPostings.PublishJobPosting;

/// <summary>
/// Command to publish a job posting, making it visible and accepting applications.
/// </summary>
public sealed record PublishJobPostingCommand(
    Guid JobPostingId) : ICommand<JobPostingResponse>;
