using FluentValidation;

namespace UniHub.Chat.Application.Commands.CreateChannel;

/// <summary>
/// Validator for CreateChannelCommand
/// </summary>
public sealed class CreateChannelCommandValidator : AbstractValidator<CreateChannelCommand>
{
    public CreateChannelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Channel name is required")
            .MinimumLength(3)
            .WithMessage("Channel name must be at least 3 characters")
            .MaximumLength(50)
            .WithMessage("Channel name cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.OwnerId)
            .NotEmpty()
            .WithMessage("OwnerId is required");
    }
}
