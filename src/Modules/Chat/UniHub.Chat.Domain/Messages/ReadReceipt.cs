using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Domain.Messages;

/// <summary>
/// ReadReceipt value object - tracks when a user read a message
/// </summary>
public sealed class ReadReceipt : ValueObject
{
    /// <summary>
    /// User ID who read the message
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Timestamp when the message was read
    /// </summary>
    public DateTime ReadAt { get; private set; }

    private ReadReceipt() { } // EF Core

    private ReadReceipt(Guid userId, DateTime readAt)
    {
        UserId = userId;
        ReadAt = readAt;
    }

    /// <summary>
    /// Create a new read receipt
    /// </summary>
    public static Result<ReadReceipt> Create(Guid userId, DateTime? readAt = null)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<ReadReceipt>(new Error(
                "ReadReceipt.InvalidUserId",
                "User ID cannot be empty"));
        }

        var timestamp = readAt ?? DateTime.UtcNow;

        if (timestamp > DateTime.UtcNow.AddMinutes(1))
        {
            return Result.Failure<ReadReceipt>(new Error(
                "ReadReceipt.InvalidTimestamp",
                "Read timestamp cannot be in the future"));
        }

        return Result.Success(new ReadReceipt(userId, timestamp));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId;
        yield return ReadAt;
    }
}
