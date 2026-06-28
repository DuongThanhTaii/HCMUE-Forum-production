using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Companies.GetCompanyById;

/// <summary>
/// Query to get company details by ID
/// </summary>
public sealed record GetCompanyByIdQuery(
    Guid CompanyId
) : IRequest<Result<CompanyDetailResponse>>;

/// <summary>
/// Response containing company details
/// </summary>
public sealed record CompanyDetailResponse(
    Guid CompanyId,
    string Name,
    string Description,
    string Industry,
    string Size,
    string? Website,
    string? LogoUrl,
    string? Location,
    string Status,
    DateTime RegisteredAt,
    int TotalJobPostings,
    int ActiveJobPostings
);
