using UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.JobPostings.CloseJobPosting;

/// <summary>
/// Command to close a job posting permanently with a reason.
/// </summary>
public sealed record CloseJobPostingCommand(
    Guid JobPostingId,
    string Reason) : ICommand<JobPostingResponse>;
