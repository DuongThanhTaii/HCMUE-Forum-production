using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.SavedJobs.IsSaved;

/// <summary>
/// Query to check if a job posting is saved by a user.
/// </summary>
public sealed record IsJobSavedQuery(
    Guid JobPostingId,
    Guid UserId
) : IRequest<Result<bool>>;
