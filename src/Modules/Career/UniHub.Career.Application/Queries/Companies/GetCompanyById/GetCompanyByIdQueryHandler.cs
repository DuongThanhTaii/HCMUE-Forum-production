using MediatR;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Companies.GetCompanyById;

/// <summary>
/// Handler for GetCompanyByIdQuery
/// </summary>
internal sealed class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, Result<CompanyDetailResponse>>
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IJobPostingRepository _jobPostingRepository;

    public GetCompanyByIdQueryHandler(
        ICompanyRepository companyRepository,
        IJobPostingRepository jobPostingRepository)
    {
        _companyRepository = companyRepository;
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<Result<CompanyDetailResponse>> Handle(
        GetCompanyByIdQuery request,
        CancellationToken cancellationToken)
    {
        var companyId = CompanyId.Create(request.CompanyId);
        var company = await _companyRepository.GetByIdAsync(companyId, cancellationToken);

        if (company is null)
        {
            return Result.Failure<CompanyDetailResponse>(
                new Error("Company.NotFound", $"Company with ID {request.CompanyId} not found"));
        }

        // Get job posting statistics
        var jobPostings = await _jobPostingRepository.GetByCompanyAsync(request.CompanyId, cancellationToken);
        var activePostings = jobPostings.Count(j => j.Status == Domain.JobPostings.JobPostingStatus.Published);

        var response = new CompanyDetailResponse(
            company.Id.Value,
            company.Name,
            company.Description,
            company.Industry.ToString(),
            company.Size.ToString(),
            company.Website,
            company.LogoUrl,
            company.ContactInfo.Address,
            company.Status.ToString(),
            company.RegisteredAt,
            jobPostings.Count,
            activePostings
        );

        return Result.Success(response);
    }
}
