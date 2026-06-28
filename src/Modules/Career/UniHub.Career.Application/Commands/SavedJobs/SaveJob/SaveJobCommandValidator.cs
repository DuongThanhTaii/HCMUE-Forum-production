using FluentValidation;

namespace UniHub.Career.Application.Commands.SavedJobs.SaveJob;

/// <summary>
/// Validator for SaveJobCommand.
/// </summary>
public sealed class SaveJobCommandValidator : AbstractValidator<SaveJobCommand>
{
    public SaveJobCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("Job posting ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
