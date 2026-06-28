using FluentValidation;

namespace UniHub.Career.Application.Commands.JobPostings.CloseJobPosting;

/// <summary>
/// Validator for CloseJobPostingCommand.
/// </summary>
public sealed class CloseJobPostingCommandValidator : AbstractValidator<CloseJobPostingCommand>
{
    public CloseJobPostingCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("JobPostingId is required");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Reason is required")
            .MaximumLength(500)
            .WithMessage("Reason must not exceed 500 characters");
    }
}
