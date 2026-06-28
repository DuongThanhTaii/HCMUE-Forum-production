using FluentValidation;

namespace UniHub.Career.Application.Queries.JobMatching.GetMatchingCandidatesForJob;

internal sealed class GetMatchingCandidatesForJobQueryValidator : AbstractValidator<GetMatchingCandidatesForJobQuery>
{
    public GetMatchingCandidatesForJobQueryValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("JobPostingId is required");

        RuleFor(x => x.CompanyId)
            .NotEmpty()
            .WithMessage("CompanyId is required");

        RuleFor(x => x.MinimumMatchPercentage)
            .InclusiveBetween(0, 100)
            .WithMessage("MinimumMatchPercentage must be between 0 and 100");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
