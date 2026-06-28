using FluentValidation;

namespace UniHub.Identity.Application.Commands.AssignScopedPermission;

internal sealed class AssignScopedPermissionCommandValidator : AbstractValidator<AssignScopedPermissionCommand>
{
    public AssignScopedPermissionCommandValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty()
            .WithMessage("Role ID is required");

        RuleFor(x => x.PermissionId)
            .NotEmpty()
            .WithMessage("Permission ID is required");

        RuleFor(x => x.ScopeType)
            .NotEmpty()
            .WithMessage("Scope type is required")
            .Must(BeValidScopeType)
            .WithMessage("Scope type must be one of: Global, Module, Course, Department, Category");

        RuleFor(x => x.ScopeValue)
            .NotEmpty()
            .When(x => x.ScopeType != "Global")
            .WithMessage("Scope value is required for non-global scopes")
            .MaximumLength(100)
            .WithMessage("Scope value cannot exceed 100 characters");

        RuleFor(x => x.ScopeValue)
            .Empty()
            .When(x => x.ScopeType == "Global")
            .WithMessage("Scope value should not be provided for Global scope");
    }

    private static bool BeValidScopeType(string scopeType)
    {
        var validTypes = new[] { "Global", "Module", "Course", "Department", "Category" };
        return validTypes.Contains(scopeType, StringComparer.OrdinalIgnoreCase);
    }
}
