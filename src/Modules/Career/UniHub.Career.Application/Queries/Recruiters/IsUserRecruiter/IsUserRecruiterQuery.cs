using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Recruiters.IsUserRecruiter;

public sealed record IsUserRecruiterQuery(
    Guid UserId,
    Guid CompanyId) : IQuery<IsRecruiterResponse>;

public sealed record IsRecruiterResponse(
    bool IsRecruiter,
    bool IsActive,
    RecruiterPermissionsDto? Permissions);

public sealed record RecruiterPermissionsDto(
    bool CanManageJobPostings,
    bool CanReviewApplications,
    bool CanUpdateApplicationStatus,
    bool CanInviteRecruiters);

internal sealed class IsUserRecruiterQueryHandler : IQueryHandler<IsUserRecruiterQuery, IsRecruiterResponse>
{
    private readonly IRecruiterRepository _recruiterRepository;

    public IsUserRecruiterQueryHandler(IRecruiterRepository recruiterRepository)
    {
        _recruiterRepository = recruiterRepository;
    }

    public async Task<Result<IsRecruiterResponse>> Handle(IsUserRecruiterQuery request, CancellationToken cancellationToken)
    {
        var companyId = CompanyId.Create(request.CompanyId);
        var recruiter = await _recruiterRepository.GetByUserAndCompanyAsync(request.UserId, companyId, cancellationToken);

        if (recruiter is null)
        {
            return Result.Success(new IsRecruiterResponse(false, false, null));
        }

        var permissions = new RecruiterPermissionsDto(
            recruiter.Permissions.CanManageJobPostings,
            recruiter.Permissions.CanReviewApplications,
            recruiter.Permissions.CanUpdateApplicationStatus,
            recruiter.Permissions.CanInviteRecruiters);

        var response = new IsRecruiterResponse(true, recruiter.IsActive(), permissions);

        return Result.Success(response);
    }
}
