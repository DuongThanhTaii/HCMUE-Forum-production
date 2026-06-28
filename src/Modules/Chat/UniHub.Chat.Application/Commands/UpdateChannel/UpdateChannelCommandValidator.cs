using FluentValidation;

namespace UniHub.Chat.Application.Commands.UpdateChannel;

/// <summary>
/// Validator for UpdateChannelCommand
/// </summary>
public sealed class UpdateChannelCommandValidator : AbstractValidator<UpdateChannelCommand>
{
    public UpdateChannelCommandValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .WithMessage("ChannelId is required");

        RuleFor(x => x.Name)
            .MinimumLength(3)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Channel name must be at least 3 characters")
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.Name))
            .WithMessage("Channel name cannot exceed 50 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Description cannot exceed 500 characters");

        RuleFor(x => x.UpdatedBy)
            .NotEmpty()
            .WithMessage("UpdatedBy is required");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Name) || !string.IsNullOrEmpty(x.Description))
            .WithMessage("At least one field (Name or Description) must be provided");
    }
}
