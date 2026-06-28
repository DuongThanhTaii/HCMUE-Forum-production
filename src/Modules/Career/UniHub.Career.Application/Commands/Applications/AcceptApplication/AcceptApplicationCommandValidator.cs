using FluentValidation;

namespace UniHub.Career.Application.Commands.Applications.AcceptApplication;

/// <summary>
/// Validator for AcceptApplicationCommand.
/// </summary>
public sealed class AcceptApplicationCommandValidator : AbstractValidator<AcceptApplicationCommand>
{
    public AcceptApplicationCommandValidator()
    {
        RuleFor(x => x.ApplicationId)
            .NotEmpty()
            .WithMessage("Application ID is required.");

        RuleFor(x => x.ApplicantId)
            .NotEmpty()
            .WithMessage("Applicant ID is required.");
    }
}
