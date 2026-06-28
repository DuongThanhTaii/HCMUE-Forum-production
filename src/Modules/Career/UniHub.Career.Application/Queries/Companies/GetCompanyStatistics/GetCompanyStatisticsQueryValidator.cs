using FluentValidation;

namespace UniHub.Career.Application.Queries.Companies.GetCompanyStatistics;

/// <summary>
/// Validator for GetCompanyStatisticsQuery.
/// </summary>
public sealed class GetCompanyStatisticsQueryValidator : AbstractValidator<GetCompanyStatisticsQuery>
{
    public GetCompanyStatisticsQueryValidator()
    {
        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("Company ID is required.");
    }
}
