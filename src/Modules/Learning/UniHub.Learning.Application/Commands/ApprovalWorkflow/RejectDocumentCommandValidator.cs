using FluentValidation;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Validator for RejectDocumentCommand
/// </summary>
public sealed class RejectDocumentCommandValidator : AbstractValidator<RejectDocumentCommand>
{
    public RejectDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.RejectorId)
            .NotEmpty().WithMessage("Rejector ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Rejection reason must not exceed 1000 characters");
    }
}
