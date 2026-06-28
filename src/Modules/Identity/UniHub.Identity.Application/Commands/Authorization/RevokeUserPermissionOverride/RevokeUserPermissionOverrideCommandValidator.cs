using FluentValidation;

namespace UniHub.Identity.Application.Commands.Authorization.RevokeUserPermissionOverride;

internal sealed class RevokeUserPermissionOverrideCommandValidator : AbstractValidator<RevokeUserPermissionOverrideCommand>
{
    public RevokeUserPermissionOverrideCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

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
            .When(x => !string.Equals(x.ScopeType, "Global", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Scope value is required for non-global scopes")
            .MaximumLength(100)
            .WithMessage("Scope value cannot exceed 100 characters");

        RuleFor(x => x.ScopeValue)
            .Empty()
            .When(x => string.Equals(x.ScopeType, "Global", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Scope value should not be provided for Global scope");
    }

    private static bool BeValidScopeType(string scopeType)
    {
        var validTypes = new[] { "Global", "Module", "Course", "Department", "Category" };
        return validTypes.Contains(scopeType, StringComparer.OrdinalIgnoreCase);
    }
}
