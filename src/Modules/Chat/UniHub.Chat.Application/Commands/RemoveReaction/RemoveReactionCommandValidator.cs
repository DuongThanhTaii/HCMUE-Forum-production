using FluentValidation;

namespace UniHub.Chat.Application.Commands.RemoveReaction;

/// <summary>
/// Validator for RemoveReactionCommand
/// </summary>
public sealed class RemoveReactionCommandValidator : AbstractValidator<RemoveReactionCommand>
{
    public RemoveReactionCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Emoji)
            .NotEmpty()
            .WithMessage("Emoji is required");
    }
}
