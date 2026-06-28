using FluentValidation;

namespace UniHub.Identity.Application.Commands.Roles.DeleteRole;

/// <summary>
/// Validator for delete role command
/// </summary>
public sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty().WithMessage("Role ID is required");
    }
}
