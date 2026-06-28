using FluentValidation;

namespace UniHub.Career.Application.Queries.JobPostings.SearchJobPostings;

internal sealed class SearchJobPostingsQueryValidator : AbstractValidator<SearchJobPostingsQuery>
{
    public SearchJobPostingsQueryValidator()
    {
        RuleFor(x => x.Keywords)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Keywords));

        RuleFor(x => x.City)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.City));

        RuleFor(x => x.MinSalary)
            .GreaterThan(0)
            .When(x => x.MinSalary.HasValue);

        RuleFor(x => x.MaxSalary)
            .GreaterThan(0)
            .When(x => x.MaxSalary.HasValue);

        RuleFor(x => x.MaxSalary)
            .GreaterThanOrEqualTo(x => x.MinSalary)
            .When(x => x.MinSalary.HasValue && x.MaxSalary.HasValue)
            .WithMessage("MaxSalary must be greater than or equal to MinSalary");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .When(x => x.MinSalary.HasValue || x.MaxSalary.HasValue)
            .WithMessage("Currency is required when salary range is specified");

        RuleFor(x => x.Currency)
            .MaximumLength(3)
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.RequiredSkills)
            .Must(skills => skills == null || skills.Count <= 20)
            .WithMessage("Cannot search for more than 20 skills at once");

        RuleForEach(x => x.RequiredSkills)
            .NotEmpty()
            .MaximumLength(100)
            .When(x => x.RequiredSkills is not null);

        RuleFor(x => x.Tags)
            .Must(tags => tags == null || tags.Count <= 20)
            .WithMessage("Cannot search for more than 20 tags at once");

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(50)
            .When(x => x.Tags is not null);

        RuleFor(x => x.PostedAfter)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.PostedAfter.HasValue)
            .WithMessage("PostedAfter cannot be in the future");

        RuleFor(x => x.PostedBefore)
            .LessThanOrEqualTo(DateTime.UtcNow)
            .When(x => x.PostedBefore.HasValue)
            .WithMessage("PostedBefore cannot be in the future");

        RuleFor(x => x.PostedBefore)
            .GreaterThanOrEqualTo(x => x.PostedAfter)
            .When(x => x.PostedAfter.HasValue && x.PostedBefore.HasValue)
            .WithMessage("PostedBefore must be after PostedAfter");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
