using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Applications.GetApplicationsByJob;

/// <summary>
/// Handler for GetApplicationsByJobQuery.
/// </summary>
internal sealed class GetApplicationsByJobQueryHandler
    : IQueryHandler<GetApplicationsByJobQuery, ApplicationListResponse>
{
    private readonly IApplicationRepository _applicationRepository;

    public GetApplicationsByJobQueryHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async Task<Result<ApplicationListResponse>> Handle(
        GetApplicationsByJobQuery query,
        CancellationToken cancellationToken)
    {
        // Parse status filter if provided
        ApplicationStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<ApplicationStatus>(query.Status, true, out var status))
                return Result.Failure<ApplicationListResponse>(
                    new Error("Application.InvalidStatus",
                        $"Invalid status: {query.Status}"));
            statusFilter = status;
        }

        // Retrieve applications
        var (applications, totalCount) = await _applicationRepository.GetByJobPostingAsync(
            JobPostingId.Create(query.JobPostingId),
            statusFilter,
            query.Page,
            query.PageSize,
            cancellationToken);

        // Map to summary DTOs
        var items = applications.Select(a => new ApplicationSummary(
            a.Id.Value,
            a.JobPostingId.Value,
            a.ApplicantId,
            a.Status.ToString(),
            a.SubmittedAt,
            a.LastStatusChangedAt,
            a.CoverLetter != null)).ToList();

        var response = new ApplicationListResponse(
            items,
            totalCount,
            query.Page,
            query.PageSize,
            (int)Math.Ceiling(totalCount / (double)query.PageSize));

        return Result.Success(response);
    }
}
