using FluentValidation;
using UniHub.Identity.Domain.Permissions;

namespace UniHub.Identity.Application.Commands.Roles.RemovePermission;

/// <summary>
/// Validator for remove permission command
/// </summary>
public sealed class RemovePermissionCommandValidator : AbstractValidator<RemovePermissionCommand>
{
    public RemovePermissionCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");

        RuleFor(x => x.PermissionId)
            .NotEmpty().WithMessage("Permission ID is required");

        RuleFor(x => x.ScopeType)
            .IsInEnum().WithMessage("Invalid scope type");

        RuleFor(x => x.ScopeValue)
            .NotEmpty().WithMessage("Scope value is required")
            .When(x => x.ScopeType != PermissionScopeType.Global);
    }
}
