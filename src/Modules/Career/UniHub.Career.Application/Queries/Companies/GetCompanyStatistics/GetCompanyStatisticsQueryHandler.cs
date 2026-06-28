using MediatR;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Companies.GetCompanyStatistics;

/// <summary>
/// Handler for GetCompanyStatisticsQuery.
/// </summary>
public sealed class GetCompanyStatisticsQueryHandler : IRequestHandler<GetCompanyStatisticsQuery, Result<CompanyStatisticsResponse>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly IApplicationRepository _applicationRepository;

    public GetCompanyStatisticsQueryHandler(
        ICompanyRepository companyRepository,
        IJobPostingRepository jobPostingRepository,
        IApplicationRepository applicationRepository)
    {
        _companyRepository = companyRepository;
        _jobPostingRepository = jobPostingRepository;
        _applicationRepository = applicationRepository;
    }

    public async Task<Result<CompanyStatisticsResponse>> Handle(
        GetCompanyStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        // Verify company exists
        var company = await _companyRepository.GetByIdAsync(
            CompanyId.Create(request.CompanyId),
            cancellationToken);

        if (company is null)
            return Result.Failure<CompanyStatisticsResponse>(
                new Error("Company.NotFound", $"Company with ID '{request.CompanyId}' was not found."));

        // Get all job postings for the company
        var jobPostings = await _jobPostingRepository.GetByCompanyAsync(
            request.CompanyId,
            cancellationToken);

        // Get all applications for the company
        var (applications, totalApplications) = await _applicationRepository.GetByCompanyAsync(
            request.CompanyId,
            status: null,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        // Calculate overview statistics
        var totalViews = jobPostings.Sum(j => j.ViewCount);
        var activeJobPostings = jobPostings.Count(j => j.Status == JobPostingStatus.Published);
        var lastJobPostedAt = jobPostings
            .Where(j => j.PublishedAt.HasValue)
            .OrderByDescending(j => j.PublishedAt)
            .FirstOrDefault()?.PublishedAt;

        var overview = new CompanyOverviewStats(
            TotalJobPostings: jobPostings.Count,
            ActiveJobPostings: activeJobPostings,
            TotalApplications: totalApplications,
            TotalViews: totalViews,
            LastJobPostedAt: lastJobPostedAt
        );

        // Calculate job posting status breakdown
        var jobPostingStats = new JobPostingStats(
            Draft: jobPostings.Count(j => j.Status == JobPostingStatus.Draft),
            Published: jobPostings.Count(j => j.Status == JobPostingStatus.Published),
            Paused: jobPostings.Count(j => j.Status == JobPostingStatus.Paused),
            Closed: jobPostings.Count(j => j.Status == JobPostingStatus.Closed),
            Expired: jobPostings.Count(j => j.Status == JobPostingStatus.Expired)
        );

        // Calculate application status breakdown
        var pendingCount = applications.Count(a => a.Status == ApplicationStatus.Pending);
        var reviewingCount = applications.Count(a => a.Status == ApplicationStatus.Reviewing);
        var shortlistedCount = applications.Count(a => a.Status == ApplicationStatus.Shortlisted);
        var interviewedCount = applications.Count(a => a.Status == ApplicationStatus.Interviewed);
        var offeredCount = applications.Count(a => a.Status == ApplicationStatus.Offered);
        var acceptedCount = applications.Count(a => a.Status == ApplicationStatus.Accepted);
        var rejectedCount = applications.Count(a => a.Status == ApplicationStatus.Rejected);
        var withdrawnCount = applications.Count(a => a.Status == ApplicationStatus.Withdrawn);

        var acceptanceRate = totalApplications > 0
            ? (double)acceptedCount / totalApplications * 100
            : 0;

        var rejectionRate = totalApplications > 0
            ? (double)rejectedCount / totalApplications * 100
            : 0;

        var applicationStats = new ApplicationStats(
            Pending: pendingCount,
            Reviewing: reviewingCount,
            Shortlisted: shortlistedCount,
            Interviewed: interviewedCount,
            Offered: offeredCount,
            Accepted: acceptedCount,
            Rejected: rejectedCount,
            Withdrawn: withdrawnCount,
            AcceptanceRate: Math.Round(acceptanceRate, 2),
            RejectionRate: Math.Round(rejectionRate, 2)
        );

        // Get top 5 performing jobs (by application count)
        var topJobs = jobPostings
            .Where(j => j.PublishedAt.HasValue)
            .OrderByDescending(j => j.ApplicationCount)
            .Take(5)
            .Select(j => new TopJobPosting(
                j.Id.Value,
                j.Title,
                j.ApplicationCount,
                j.ViewCount,
                j.PublishedAt!.Value))
            .ToList();

        var response = new CompanyStatisticsResponse(
            request.CompanyId,
            company.Name,
            overview,
            jobPostingStats,
            applicationStats,
            topJobs
        );

        return Result.Success(response);
    }
}
