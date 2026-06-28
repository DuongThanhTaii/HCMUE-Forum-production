using FluentValidation;

namespace UniHub.Chat.Application.Commands.AddReaction;

/// <summary>
/// Validator for AddReactionCommand
/// </summary>
public sealed class AddReactionCommandValidator : AbstractValidator<AddReactionCommand>
{
    private static readonly string[] AllowedEmojis = new[]
    {
        // Common reactions
        "👍", "👎", "❤️", "😂", "😮", "😢", "😡", "🎉", "🔥", "👏",
        "✅", "❌", "⭐", "💯", "🙏", "💪", "👀", "🤔", "😍", "🥳"
    };

    public AddReactionCommandValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Emoji)
            .NotEmpty()
            .WithMessage("Emoji is required")
            .Must(BeAllowedEmoji)
            .WithMessage($"Emoji must be one of the supported emojis: {string.Join(", ", AllowedEmojis)}");
    }

    private bool BeAllowedEmoji(string emoji)
    {
        return AllowedEmojis.Contains(emoji);
    }
}
