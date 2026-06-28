using MediatR;
using UniHub.Career.Application.Abstractions;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.SavedJobs.GetSavedJobs;

/// <summary>
/// Handler for GetSavedJobsQuery.
/// </summary>
public sealed class GetSavedJobsQueryHandler : IRequestHandler<GetSavedJobsQuery, Result<SavedJobsResponse>>
{
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly ICompanyRepository _companyRepository;

    public GetSavedJobsQueryHandler(
        ISavedJobRepository savedJobRepository,
        IJobPostingRepository jobPostingRepository,
        ICompanyRepository companyRepository)
    {
        _savedJobRepository = savedJobRepository;
        _jobPostingRepository = jobPostingRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<SavedJobsResponse>> Handle(GetSavedJobsQuery request, CancellationToken cancellationToken)
    {
        // Get saved jobs with pagination
        var savedJobs = await _savedJobRepository.GetSavedJobsByUserAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _savedJobRepository.GetSavedCountAsync(
            request.UserId,
            cancellationToken);

        var savedJobDtos = new List<SavedJobDto>();

        // Enrich with job posting and company details
        foreach (var savedJob in savedJobs)
        {
            var jobPosting = await _jobPostingRepository.GetByIdAsync(
                Domain.JobPostings.JobPostingId.Create(savedJob.JobPostingId),
                cancellationToken);

            if (jobPosting is null)
                continue; // Skip if job posting was deleted

            var company = await _companyRepository.GetByIdAsync(
                Domain.Companies.CompanyId.Create(jobPosting.CompanyId),
                cancellationToken);

            if (company is null)
                continue; // Skip if company was deleted

            savedJobDtos.Add(new SavedJobDto(
                savedJob.JobPostingId,
                jobPosting.Title,
                jobPosting.CompanyId,
                company.Name,
                jobPosting.JobType.ToString(),
                jobPosting.ExperienceLevel.ToString(),
                jobPosting.Status.ToString(),
                new LocationDto(
                    jobPosting.Location.City,
                    jobPosting.Location.District,
                    jobPosting.Location.IsRemote),
                jobPosting.Salary is not null
                    ? new SalaryDto(
                        jobPosting.Salary.MinAmount,
                        jobPosting.Salary.MaxAmount,
                        jobPosting.Salary.Currency.ToString(),
                        jobPosting.Salary.Period.ToString())
                    : null,
                jobPosting.PublishedAt,
                jobPosting.Deadline,
                jobPosting.ViewCount,
                jobPosting.ApplicationCount,
                savedJob.SavedAt));
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new SavedJobsResponse(
            savedJobDtos,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages);

        return Result.Success(response);
    }
}
