using FluentValidation;

namespace UniHub.Career.Application.Commands.JobPostings.PublishJobPosting;

/// <summary>
/// Validator for PublishJobPostingCommand.
/// </summary>
public sealed class PublishJobPostingCommandValidator : AbstractValidator<PublishJobPostingCommand>
{
    public PublishJobPostingCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("JobPostingId is required");
    }
}
