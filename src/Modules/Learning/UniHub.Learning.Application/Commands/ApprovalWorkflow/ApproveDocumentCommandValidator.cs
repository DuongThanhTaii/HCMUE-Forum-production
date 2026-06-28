using FluentValidation;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Validator for ApproveDocumentCommand
/// </summary>
public sealed class ApproveDocumentCommandValidator : AbstractValidator<ApproveDocumentCommand>
{
    public ApproveDocumentCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.ApproverId)
            .NotEmpty().WithMessage("Approver ID is required");

        RuleFor(x => x.ApprovalNotes)
            .MaximumLength(1000).WithMessage("Approval notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.ApprovalNotes));
    }
}
