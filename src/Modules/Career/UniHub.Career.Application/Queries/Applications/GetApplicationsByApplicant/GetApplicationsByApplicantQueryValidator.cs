using FluentValidation;

namespace UniHub.Career.Application.Queries.Applications.GetApplicationsByApplicant;

/// <summary>
/// Validator for GetApplicationsByApplicantQuery.
/// </summary>
public sealed class GetApplicationsByApplicantQueryValidator : AbstractValidator<GetApplicationsByApplicantQuery>
{
    public GetApplicationsByApplicantQueryValidator()
    {
        RuleFor(x => x.ApplicantId)
            .NotEmpty()
            .WithMessage("Applicant ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
