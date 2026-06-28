using FluentValidation;

namespace UniHub.Career.Application.Queries.Applications.GetApplicationsByJob;

/// <summary>
/// Validator for GetApplicationsByJobQuery.
/// </summary>
public sealed class GetApplicationsByJobQueryValidator : AbstractValidator<GetApplicationsByJobQuery>
{
    public GetApplicationsByJobQueryValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("Job posting ID is required.");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");
    }
}
