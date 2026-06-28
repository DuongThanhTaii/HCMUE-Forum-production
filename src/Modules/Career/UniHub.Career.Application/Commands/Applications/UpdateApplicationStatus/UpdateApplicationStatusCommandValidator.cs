using FluentValidation;

namespace UniHub.Career.Application.Commands.Applications.UpdateApplicationStatus;

/// <summary>
/// Validator for UpdateApplicationStatusCommand.
/// </summary>
public sealed class UpdateApplicationStatusCommandValidator : AbstractValidator<UpdateApplicationStatusCommand>
{
    private static readonly string[] AllowedStatuses =
    {
        "Reviewing", "Shortlisted", "Interviewed", "Offered"
    };

    public UpdateApplicationStatusCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application ID is required.");

        RuleFor(x => x.ReviewerId)
            .NotEmpty()
            .WithMessage("Reviewer ID is required.");

        RuleFor(x => x.TargetStatus)
            .NotEmpty()
            .WithMessage("Target status is required.")
            .Must(status => AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Target status must be one of: {string.Join(", ", AllowedStatuses)}");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Notes))
            .WithMessage("Notes must not exceed 1000 characters.");
    }
}
