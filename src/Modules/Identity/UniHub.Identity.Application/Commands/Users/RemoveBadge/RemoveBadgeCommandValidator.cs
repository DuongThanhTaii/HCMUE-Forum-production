using FluentValidation;

namespace UniHub.Identity.Application.Commands.Users.RemoveBadge;

/// <summary>
/// Validator for remove badge command
/// </summary>
public sealed class RemoveBadgeCommandValidator : AbstractValidator<RemoveBadgeCommand>
{
    public RemoveBadgeCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
