using FluentValidation;

namespace UniHub.Forum.Application.Commands.ReportPost;

public sealed class ReportPostCommandValidator : AbstractValidator<ReportPostCommand>
{
    public ReportPostCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("PostId cannot be empty");

        RuleFor(x => x.ReporterId)
            .NotEmpty().WithMessage("ReporterId cannot be empty");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Invalid report reason");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
