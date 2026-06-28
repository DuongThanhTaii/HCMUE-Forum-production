using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Companies.RegisterCompany;

/// <summary>
/// Handler for company registration command.
/// </summary>
public sealed class RegisterCompanyCommandHandler : ICommandHandler<RegisterCompanyCommand, CompanyResponse>
{
    private readonly ICompanyRepository _companyRepository;

    public RegisterCompanyCommandHandler(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<Result<CompanyResponse>> Handle(
        RegisterCompanyCommand request,
        CancellationToken cancellationToken)
    {
        // Check company name uniqueness
        var isNameUnique = await _companyRepository.IsNameUniqueAsync(request.Name, cancellationToken);
        if (!isNameUnique)
        {
            return Result.Failure<CompanyResponse>(
                new Error("Company.NameNotUnique", "A company with this name already exists."));
        }

        // Create contact info value object
        var contactInfoResult = ContactInfo.Create(request.Email, request.Phone, request.Address);
        if (contactInfoResult.IsFailure)
        {
            return Result.Failure<CompanyResponse>(contactInfoResult.Error);
        }

        // Create social links value object
        var socialLinksResult = SocialLinks.Create(
            request.LinkedIn,
            request.Facebook,
            request.Twitter,
            request.Instagram,
            request.YouTube);
        if (socialLinksResult.IsFailure)
        {
            return Result.Failure<CompanyResponse>(socialLinksResult.Error);
        }

        // Register company
        var companyResult = Company.Register(
            request.Name,
            request.Description,
            request.Industry,
            request.Size,
            contactInfoResult.Value,
            request.RegisteredBy,
            request.Website,
            request.LogoUrl,
            request.FoundedYear,
            socialLinksResult.Value);

        if (companyResult.IsFailure)
        {
            return Result.Failure<CompanyResponse>(companyResult.Error);
        }

        var company = companyResult.Value;

        // Save company
        await _companyRepository.AddAsync(company, cancellationToken);

        // Map to response DTO
        var response = new CompanyResponse(
            company.Id.Value,
            company.Name,
            company.Description,
            company.Industry.ToString(),
            company.Size.ToString(),
            company.Status.ToString(),
            company.ContactInfo,
            company.Website,
            company.LogoUrl,
            company.FoundedYear,
            company.RegisteredAt);

        return Result.Success(response);
    }
}
