using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.SavedJobs.SaveJob;

/// <summary>
/// Command to save a job posting to user's saved jobs list.
/// </summary>
public sealed record SaveJobCommand(
    Guid JobPostingId,
    Guid UserId
) : IRequest<Result>;
