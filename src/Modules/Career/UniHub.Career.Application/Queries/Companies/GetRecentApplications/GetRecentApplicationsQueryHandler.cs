using MediatR;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.Results;
using DomainApplication = UniHub.Career.Domain.Applications.Application;

namespace UniHub.Career.Application.Queries.Companies.GetRecentApplications;

/// <summary>
/// Handler for GetRecentApplicationsQuery.
/// </summary>
public sealed class GetRecentApplicationsQueryHandler : IRequestHandler<GetRecentApplicationsQuery, Result<RecentApplicationsResponse>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobPostingRepository _jobPostingRepository;

    public GetRecentApplicationsQueryHandler(
        ICompanyRepository companyRepository,
        IApplicationRepository applicationRepository,
        IJobPostingRepository jobPostingRepository)
    {
        _companyRepository = companyRepository;
        _applicationRepository = applicationRepository;
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<Result<RecentApplicationsResponse>> Handle(
        GetRecentApplicationsQuery request,
        CancellationToken cancellationToken)
    {
        // Verify company exists
        var company = await _companyRepository.GetByIdAsync(
            CompanyId.Create(request.CompanyId),
            cancellationToken);

        if (company is null)
            return Result.Failure<RecentApplicationsResponse>(
                new Error("Company.NotFound", $"Company with ID '{request.CompanyId}' was not found."));

        // Get applications for this company
        var (applications, totalCount) = await _applicationRepository.GetByCompanyAsync(
            request.CompanyId,
            status: null,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken);

        var applicationDtos = new List<ApplicationSummaryDto>();

        // Enrich with job posting details
        foreach (var application in applications)
        {
            var jobPosting = await _jobPostingRepository.GetByIdAsync(
                application.JobPostingId,
                cancellationToken);

            if (jobPosting is null)
                continue;

            applicationDtos.Add(new ApplicationSummaryDto(
                application.Id.Value,
                application.JobPostingId.Value,
                jobPosting.Title,
                application.ApplicantId,
                $"Applicant {application.ApplicantId}", // TODO: Fetch from User context when available
                application.Status.ToString(),
                application.SubmittedAt,
                application.LastStatusChangedAt,
                application.CoverLetter is not null
            ));
        }

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new RecentApplicationsResponse(
            applicationDtos,
            totalCount,
            request.Page,
            request.PageSize,
            totalPages
        );

        return Result.Success(response);
    }
}
