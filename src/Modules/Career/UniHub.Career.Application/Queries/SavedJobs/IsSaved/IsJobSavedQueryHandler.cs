using MediatR;
using UniHub.Career.Application.Abstractions;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.SavedJobs.IsSaved;

/// <summary>
/// Handler for IsJobSavedQuery.
/// </summary>
public sealed class IsJobSavedQueryHandler : IRequestHandler<IsJobSavedQuery, Result<bool>>
{
    private readonly ISavedJobRepository _savedJobRepository;

    public IsJobSavedQueryHandler(ISavedJobRepository savedJobRepository)
    {
        _savedJobRepository = savedJobRepository;
    }

    public async Task<Result<bool>> Handle(IsJobSavedQuery request, CancellationToken cancellationToken)
    {
        var isSaved = await _savedJobRepository.IsSavedAsync(
            request.UserId,
            request.JobPostingId,
            cancellationToken);

        return Result.Success(isSaved);
    }
}
