using FluentValidation;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Application.Commands.Users.AssignBadge;

/// <summary>
/// Validator for assign badge command
/// </summary>
public sealed class AssignBadgeCommandValidator : AbstractValidator<AssignBadgeCommand>
{
    public AssignBadgeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");

        RuleFor(x => x.BadgeType)
            .IsInEnum().WithMessage("Invalid badge type");

        RuleFor(x => x.BadgeName)
            .NotEmpty().WithMessage("Badge name is required")
            .MaximumLength(100).WithMessage("Badge name cannot exceed 100 characters");

        RuleFor(x => x.VerifiedBy)
            .NotEmpty().WithMessage("Verified by is required")
            .MaximumLength(100).WithMessage("Verified by cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Description cannot exceed 200 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}
