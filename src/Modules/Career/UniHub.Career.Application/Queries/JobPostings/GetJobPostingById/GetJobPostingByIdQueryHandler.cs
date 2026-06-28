using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.JobPostings.GetJobPostingById;

/// <summary>
/// Handler for GetJobPostingByIdQuery.
/// </summary>
public sealed class GetJobPostingByIdQueryHandler
    : IQueryHandler<GetJobPostingByIdQuery, JobPostingResponse>
{
    private readonly IJobPostingRepository _jobPostingRepository;

    public GetJobPostingByIdQueryHandler(IJobPostingRepository jobPostingRepository)
    {
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<Result<JobPostingResponse>> Handle(
        GetJobPostingByIdQuery query,
        CancellationToken cancellationToken)
    {
        var jobPosting = await _jobPostingRepository.GetByIdAsync(
            JobPostingId.Create(query.JobPostingId), 
            cancellationToken);

        if (jobPosting == null)
            return Result.Failure<JobPostingResponse>(
                new Error("JobPosting.NotFound", $"Job posting with ID {query.JobPostingId} not found"));

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
