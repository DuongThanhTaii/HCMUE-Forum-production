using FluentValidation;

namespace UniHub.Career.Application.Commands.Applications.WithdrawApplication;

/// <summary>
/// Validator for WithdrawApplicationCommand.
/// </summary>
public sealed class WithdrawApplicationCommandValidator : AbstractValidator<WithdrawApplicationCommand>
{
    public WithdrawApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application ID is required.");

        RuleFor(x => x.ApplicantId)
            .NotEmpty()
            .WithMessage("Applicant ID is required.");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => !string.IsNullOrWhiteSpace(x.Reason))
            .WithMessage("Reason must not exceed 500 characters.");
    }
}
