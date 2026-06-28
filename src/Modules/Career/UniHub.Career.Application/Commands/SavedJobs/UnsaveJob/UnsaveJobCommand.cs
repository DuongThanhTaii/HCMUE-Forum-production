using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.SavedJobs.UnsaveJob;

/// <summary>
/// Command to remove a job posting from user's saved jobs list.
/// </summary>
public sealed record UnsaveJobCommand(
    Guid JobPostingId,
    Guid UserId
) : IRequest<Result>;
