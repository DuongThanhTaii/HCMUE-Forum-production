using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.Recruiters.AddRecruiter;
using UniHub.Career.Domain.Companies;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Recruiters.GetRecruitersForCompany;

public sealed record GetRecruitersForCompanyQuery(
    Guid CompanyId,
    bool ActiveOnly = false) : IQuery<RecruitersResponse>;

public sealed record RecruitersResponse(
    List<RecruiterDto> Recruiters,
    int TotalCount);

public sealed record RecruiterDto(
    Guid RecruiterId,
    Guid UserId,
    string Status,
    RecruiterPermissionsDto Permissions,
    DateTime AddedAt);

internal sealed class GetRecruitersForCompanyQueryHandler : IQueryHandler<GetRecruitersForCompanyQuery, RecruitersResponse>
{
    private readonly IRecruiterRepository _recruiterRepository;

    public GetRecruitersForCompanyQueryHandler(IRecruiterRepository recruiterRepository)
    {
        _recruiterRepository = recruiterRepository;
    }

    public async Task<Result<RecruitersResponse>> Handle(GetRecruitersForCompanyQuery request, CancellationToken cancellationToken)
    {
        var companyId = CompanyId.Create(request.CompanyId);

        var recruiters = request.ActiveOnly
            ? await _recruiterRepository.GetActiveByCompanyAsync(companyId, cancellationToken)
            : await _recruiterRepository.GetByCompanyAsync(companyId, cancellationToken);

        var recruiterDtos = recruiters.Select(r => new RecruiterDto(
            r.Id.Value,
            r.UserId,
            r.Status.ToString(),
            new RecruiterPermissionsDto(
                r.Permissions.CanManageJobPostings,
                r.Permissions.CanReviewApplications,
                r.Permissions.CanUpdateApplicationStatus,
                r.Permissions.CanInviteRecruiters),
            r.AddedAt)).ToList();

        var response = new RecruitersResponse(recruiterDtos, recruiterDtos.Count);

        return Result.Success(response);
    }
}
