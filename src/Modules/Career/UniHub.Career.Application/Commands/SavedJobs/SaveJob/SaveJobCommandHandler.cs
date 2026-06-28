using MediatR;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.SavedJobs.SaveJob;

/// <summary>
/// Handler for SaveJobCommand.
/// </summary>
public sealed class SaveJobCommandHandler : IRequestHandler<SaveJobCommand, Result>
{
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly IJobPostingRepository _jobPostingRepository;

    public SaveJobCommandHandler(
        ISavedJobRepository savedJobRepository,
        IJobPostingRepository jobPostingRepository)
    {
        _savedJobRepository = savedJobRepository;
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<Result> Handle(SaveJobCommand request, CancellationToken cancellationToken)
    {
        // Verify job posting exists
        var jobPosting = await _jobPostingRepository.GetByIdAsync(
            JobPostingId.Create(request.JobPostingId),
            cancellationToken);

        if (jobPosting is null)
            return Result.Failure(new Error("JobPosting.NotFound", $"Job posting with ID '{request.JobPostingId}' was not found."));

        // Check if already saved
        var isSaved = await _savedJobRepository.IsSavedAsync(
            request.UserId,
            request.JobPostingId,
            cancellationToken);

        if (isSaved)
            return Result.Failure(new Error("SavedJob.AlreadyExists", "This job posting is already in your saved jobs."));

        // Save the job
        await _savedJobRepository.SaveJobAsync(
            request.UserId,
            request.JobPostingId,
            cancellationToken);

        return Result.Success();
    }
}
