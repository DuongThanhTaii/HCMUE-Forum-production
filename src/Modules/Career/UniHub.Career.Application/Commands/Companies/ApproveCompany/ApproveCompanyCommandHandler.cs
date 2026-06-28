using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Companies.ApproveCompany;

internal sealed class ApproveCompanyCommandHandler : ICommandHandler<ApproveCompanyCommand>
{
    private readonly ICompanyRepository _companyRepository;

    public ApproveCompanyCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result> Handle(ApproveCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(
            CompanyId.Create(request.CompanyId),
            cancellationToken);

        if (company is null)
        {
            return Result.Failure(new Error("Company.NotFound", $"Company with ID {request.CompanyId} was not found"));
        }

        var verifyResult = company.Verify(request.ApprovedBy);
        if (verifyResult.IsFailure)
        {
            return Result.Failure(verifyResult.Error);
        }

        await _companyRepository.UpdateAsync(company, cancellationToken);
        return Result.Success();
    }
}
