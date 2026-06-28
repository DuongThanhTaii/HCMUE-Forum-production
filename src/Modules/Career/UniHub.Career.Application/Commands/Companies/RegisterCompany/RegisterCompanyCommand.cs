using UniHub.Career.Domain.Companies;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.Companies.RegisterCompany;

/// <summary>
/// Command to register a new company.
/// </summary>
public sealed record RegisterCompanyCommand(
    string Name,
    string Description,
    Industry Industry,
    CompanySize Size,
    string Email,
    string? Phone,
    string? Address,
    Guid RegisteredBy,
    string? Website = null,
    string? LogoUrl = null,
    int? FoundedYear = null,
    string? LinkedIn = null,
    string? Facebook = null,
    string? Twitter = null,
    string? Instagram = null,
    string? YouTube = null) : ICommand<CompanyResponse>;
