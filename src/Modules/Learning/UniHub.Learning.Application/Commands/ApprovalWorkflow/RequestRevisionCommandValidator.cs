using FluentValidation;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Validator for RequestRevisionCommand
/// </summary>
public sealed class RequestRevisionCommandValidator : AbstractValidator<RequestRevisionCommand>
{
    public RequestRevisionCommandValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.ReviewerId)
            .NotEmpty().WithMessage("Reviewer ID is required");

        RuleFor(x => x.RevisionNotes)
            .NotEmpty().WithMessage("Revision notes are required")
            .MinimumLength(10).WithMessage("Revision notes must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Revision notes must not exceed 1000 characters");
    }
}
