using FluentValidation;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Validator for StartReviewCommand
/// </summary>
public sealed class StartReviewCommandValidator : AbstractValidator<StartReviewCommand>
{
    public StartReviewCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required");
    }
}
