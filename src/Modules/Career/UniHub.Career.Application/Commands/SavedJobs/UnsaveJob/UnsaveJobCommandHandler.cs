using MediatR;
using UniHub.Career.Application.Abstractions;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.SavedJobs.UnsaveJob;

/// <summary>
/// Handler for UnsaveJobCommand.
/// </summary>
public sealed class UnsaveJobCommandHandler : IRequestHandler<UnsaveJobCommand, Result>
{
    private readonly ISavedJobRepository _savedJobRepository;

    public UnsaveJobCommandHandler(ISavedJobRepository savedJobRepository)
    {
        _savedJobRepository = savedJobRepository;
    }

    public async Task<Result> Handle(UnsaveJobCommand request, CancellationToken cancellationToken)
    {
        // Check if the job is saved
        var isSaved = await _savedJobRepository.IsSavedAsync(
            request.UserId,
            request.JobPostingId,
            cancellationToken);

        if (!isSaved)
            return Result.Failure(new Error("SavedJob.NotFound", "This job posting is not in your saved jobs."));

        // Remove the saved job
        await _savedJobRepository.UnsaveJobAsync(
            request.UserId,
            request.JobPostingId,
            cancellationToken);

        return Result.Success();
    }
}
