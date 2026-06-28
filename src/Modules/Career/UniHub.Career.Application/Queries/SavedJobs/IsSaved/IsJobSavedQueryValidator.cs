using FluentValidation;

namespace UniHub.Career.Application.Queries.SavedJobs.IsSaved;

/// <summary>
/// Validator for IsJobSavedQuery.
/// </summary>
public sealed class IsJobSavedQueryValidator : AbstractValidator<IsJobSavedQuery>
{
    public IsJobSavedQueryValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("Job posting ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
