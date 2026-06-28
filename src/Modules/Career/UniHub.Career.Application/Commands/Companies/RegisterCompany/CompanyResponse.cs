using UniHub.Career.Domain.Companies;

namespace UniHub.Career.Application.Commands.Companies.RegisterCompany;

/// <summary>
/// Response DTO for company registration.
/// </summary>
public sealed record CompanyResponse(
    Guid CompanyId,
    string Name,
    string Description,
    string Industry,
    string Size,
    string Status,
    ContactInfo ContactInfo,
    string? Website,
    string? LogoUrl,
    int? FoundedYear,
    DateTime RegisteredAt);
