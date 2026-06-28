using FluentValidation;

namespace UniHub.Identity.Application.Commands.Authorization.UpsertUserPermissionOverride;

internal sealed class UpsertUserPermissionOverrideCommandValidator : AbstractValidator<UpsertUserPermissionOverrideCommand>
{
    public UpsertUserPermissionOverrideCommandValidator()
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

        RuleFor(x => x.Effect)
            .NotEmpty()
            .WithMessage("Effect is required")
            .Must(BeValidEffect)
            .WithMessage("Effect must be one of: Allow, Deny");

        RuleFor(x => x.Reason)
            .MaximumLength(500)
            .When(x => x.Reason is not null)
            .WithMessage("Reason cannot exceed 500 characters");
    }

    private static bool BeValidScopeType(string scopeType)
    {
        var validTypes = new[] { "Global", "Module", "Course", "Department", "Category" };
        return validTypes.Contains(scopeType, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidEffect(string effect)
    {
        return string.Equals(effect, "Allow", StringComparison.OrdinalIgnoreCase)
               || string.Equals(effect, "Deny", StringComparison.OrdinalIgnoreCase);
    }
}
