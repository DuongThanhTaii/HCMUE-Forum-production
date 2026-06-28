using FluentValidation;

namespace UniHub.Career.Application.Commands.Applications.RejectApplication;

/// <summary>
/// Validator for RejectApplicationCommand.
/// </summary>
public sealed class RejectApplicationCommandValidator : AbstractValidator<RejectApplicationCommand>
{
    public RejectApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application ID is required.");

        RuleFor(x => x.ReviewerId)
            .NotEmpty()
            .WithMessage("Reviewer ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("Reason must not exceed 1000 characters.");
    }
}
