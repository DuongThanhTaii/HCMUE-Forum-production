using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.JobPostings.CloseJobPosting;

/// <summary>
/// Handler for closing a job posting.
/// </summary>
public sealed class CloseJobPostingCommandHandler
    : ICommandHandler<CloseJobPostingCommand, JobPostingResponse>
{
    private readonly IJobPostingRepository _jobPostingRepository;

    public CloseJobPostingCommandHandler(IJobPostingRepository jobPostingRepository)
    {
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<Result<JobPostingResponse>> Handle(
        CloseJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        // Retrieve job posting
        var jobPosting = await _jobPostingRepository.GetByIdAsync(
            JobPostingId.Create(command.JobPostingId), 
            cancellationToken);

        if (jobPosting == null)
            return Result.Failure<JobPostingResponse>(
                new Error("JobPosting.NotFound", $"Job posting with ID {command.JobPostingId} not found"));

        // Close job posting
        var closeResult = jobPosting.Close(command.Reason);

        if (closeResult.IsFailure)
            return Result.Failure<JobPostingResponse>(closeResult.Error);

        // Persist changes
        await _jobPostingRepository.UpdateAsync(jobPosting, cancellationToken);

        // Map to response DTO
        var response = MapToResponse(jobPosting);

        return Result.Success(response);
    }

    private static JobPostingResponse MapToResponse(JobPosting jobPosting)
    {
        return new JobPostingResponse(
            jobPosting.Id.Value,
            jobPosting.Title,
            jobPosting.Description,
            jobPosting.CompanyId,
            jobPosting.PostedBy,
            jobPosting.JobType.ToString(),
            jobPosting.ExperienceLevel.ToString(),
            jobPosting.Status.ToString(),
            jobPosting.Salary != null 
                ? new SalaryInfo(
                    jobPosting.Salary.MinAmount,
                    jobPosting.Salary.MaxAmount,
                    jobPosting.Salary.Currency,
                    jobPosting.Salary.Period)
                : null,
            new LocationInfo(
                jobPosting.Location.City,
                jobPosting.Location.District,
                jobPosting.Location.Address,
                jobPosting.Location.IsRemote),
            jobPosting.Deadline,
            jobPosting.CreatedAt,
            jobPosting.UpdatedAt,
            jobPosting.PublishedAt,
            jobPosting.ViewCount,
            jobPosting.ApplicationCount,
            jobPosting.Tags.ToList());
    }
}
