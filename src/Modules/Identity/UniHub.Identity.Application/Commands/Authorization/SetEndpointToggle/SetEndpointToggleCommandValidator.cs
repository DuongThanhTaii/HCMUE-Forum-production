using FluentValidation;

namespace UniHub.Identity.Application.Commands.Authorization.SetEndpointToggle;

internal sealed class SetEndpointToggleCommandValidator : AbstractValidator<SetEndpointToggleCommand>
{
    public SetEndpointToggleCommandValidator()
    {
        RuleFor(x => x.EndpointKey)
            .NotEmpty()
            .WithMessage("Endpoint key is required")
            .MaximumLength(200)
            .WithMessage("Endpoint key cannot exceed 200 characters");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("UpdatedBy is required")
            .MaximumLength(200)
            .WithMessage("UpdatedBy cannot exceed 200 characters");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .When(x => !x.IsEnabled)
            .WithMessage("Reason is required when disabling an endpoint")
            .MaximumLength(500)
            .When(x => x.Reason is not null)
            .WithMessage("Reason cannot exceed 500 characters");
    }
}
