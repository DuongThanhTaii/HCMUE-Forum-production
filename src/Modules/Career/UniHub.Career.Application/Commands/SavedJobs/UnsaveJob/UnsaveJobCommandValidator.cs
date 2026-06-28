using FluentValidation;

namespace UniHub.Career.Application.Commands.SavedJobs.UnsaveJob;

/// <summary>
/// Validator for UnsaveJobCommand.
/// </summary>
public sealed class UnsaveJobCommandValidator : AbstractValidator<UnsaveJobCommand>
{
    public UnsaveJobCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("Job posting ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
