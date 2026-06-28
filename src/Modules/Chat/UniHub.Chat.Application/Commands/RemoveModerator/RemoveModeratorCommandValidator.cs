using FluentValidation;

namespace UniHub.Chat.Application.Commands.RemoveModerator;

/// <summary>
/// Validator for RemoveModeratorCommand
/// </summary>
public sealed class RemoveModeratorCommandValidator : AbstractValidator<RemoveModeratorCommand>
{
    public RemoveModeratorCommandValidator()
    {
        RuleFor(x => x.ChannelId)
            .NotEmpty()
            .WithMessage("ChannelId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.RemovedBy)
            .NotEmpty()
            .WithMessage("RemovedBy is required");
    }
}
