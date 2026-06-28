using FluentValidation;

namespace UniHub.Career.Application.Queries.JobMatching.GetMatchingJobsForUser;

internal sealed class GetMatchingJobsForUserQueryValidator : AbstractValidator<GetMatchingJobsForUserQuery>
{
    public GetMatchingJobsForUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Skills)
            .NotNull()
            .WithMessage("Skills list cannot be null")
            .Must(skills => skills.Any())
            .WithMessage("At least one skill must be provided for matching");

        RuleFor(x => x.MinSalary)
            .GreaterThan(0)
            .When(x => x.MinSalary.HasValue)
            .WithMessage("Minimum salary must be greater than 0");

        RuleFor(x => x.MaxSalary)
            .GreaterThan(0)
            .When(x => x.MaxSalary.HasValue)
            .WithMessage("Maximum salary must be greater than 0")
            .GreaterThanOrEqualTo(x => x.MinSalary ?? 0)
            .When(x => x.MaxSalary.HasValue && x.MinSalary.HasValue)
            .WithMessage("Maximum salary must be greater than or equal to minimum salary");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize must be between 1 and 100");
    }
}
