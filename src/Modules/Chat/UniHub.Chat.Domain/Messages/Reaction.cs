using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Domain.Messages;

/// <summary>
/// Value object cho message reaction (emoji)
/// </summary>
public sealed class Reaction : ValueObject
{
    /// <summary>
    /// User ID của người react
    /// </summary>
    public Guid UserId { get; private set; }
    
    /// <summary>
    /// Emoji (👍, ❤️, 😂, etc.)
    /// </summary>
    public string Emoji { get; private set; }
    
    /// <summary>
    /// Thời gian react
    /// </summary>
    public DateTime ReactedAt { get; private set; }

    private Reaction() { } // EF Core

    private Reaction(Guid userId, string emoji, DateTime reactedAt)
    {
        UserId = userId;
        Emoji = emoji;
        ReactedAt = reactedAt;
    }

    public static Result<Reaction> Create(Guid userId, string emoji)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Reaction>(new Error("Reaction.InvalidUser", "User ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(emoji))
        {
            return Result.Failure<Reaction>(new Error("Reaction.InvalidEmoji", "Emoji cannot be empty"));
        }

        // Emoji should be 1-10 characters (supporting multi-byte emojis)
        if (emoji.Length > 10)
        {
            return Result.Failure<Reaction>(new Error("Reaction.EmojiTooLong", "Emoji cannot exceed 10 characters"));
        }

        return Result.Success(new Reaction(userId, emoji, DateTime.UtcNow));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return UserId;
        yield return Emoji;
    }
}
