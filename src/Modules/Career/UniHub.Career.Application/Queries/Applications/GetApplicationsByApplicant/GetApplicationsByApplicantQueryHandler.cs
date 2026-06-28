using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Applications.GetApplicationsByApplicant;

/// <summary>
/// Handler for GetApplicationsByApplicantQuery.
/// </summary>
internal sealed class GetApplicationsByApplicantQueryHandler
    : IQueryHandler<GetApplicationsByApplicantQuery, ApplicationsByApplicantResponse>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly ICompanyRepository _companyRepository;

    public GetApplicationsByApplicantQueryHandler(
        IApplicationRepository applicationRepository,
        IJobPostingRepository jobPostingRepository,
        ICompanyRepository companyRepository)
    {
        _applicationRepository = applicationRepository;
        _jobPostingRepository = jobPostingRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<ApplicationsByApplicantResponse>> Handle(
        GetApplicationsByApplicantQuery query,
        CancellationToken cancellationToken)
    {
        // Parse status filter if provided
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<ApplicationStatus>(query.Status, true, out var status))
                return Result.Failure<ApplicationsByApplicantResponse>(
                    new Error("Application.InvalidStatus",
                        $"Invalid status: {query.Status}"));
            statusFilter = status;
        }

        // Retrieve applications
        var (applications, totalCount) = await _applicationRepository.GetByApplicantAsync(
            query.ApplicantId,
            statusFilter,
            query.Page,
            query.PageSize,
            cancellationToken);

        // Map to summary DTOs with job posting and company info
        var items = new List<ApplicantApplicationSummary>();
        foreach (var application in applications)
        {
            var jobPosting = await _jobPostingRepository.GetByIdAsync(
                application.JobPostingId,
                cancellationToken);

            if (jobPosting == null) continue;

            var company = await _companyRepository.GetByIdAsync(
                Domain.Companies.CompanyId.Create(jobPosting.CompanyId),
                cancellationToken);

            items.Add(new ApplicantApplicationSummary(
                application.Id.Value,
                application.JobPostingId.Value,
                jobPosting.Title,
                company?.Name ?? "Unknown Company",
                application.Status.ToString(),
                application.SubmittedAt,
                application.LastStatusChangedAt,
                application.CoverLetter != null));
        }

        var response = new ApplicationsByApplicantResponse(
            items,
            totalCount,
            query.Page,
            query.PageSize,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));

        return Result.Success(response);
    }
}
