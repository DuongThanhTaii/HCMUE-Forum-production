using FluentValidation;

namespace UniHub.Identity.Application.Commands.Roles.CreateRole;

/// <summary>
/// Validator for create role command
/// </summary>
public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Role name is required")
            .MinimumLength(2).WithMessage("Role name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Role name cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Role description is required")
            .MaximumLength(200).WithMessage("Role description cannot exceed 200 characters");
    }
}
