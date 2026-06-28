using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Users;

/// <summary>
/// Directed block: blocker cannot interact with blocked user in DM (enforced in Chat).
/// </summary>
public sealed class UserBlock
{
    public Guid BlockerUserId { get; private set; }
    public Guid BlockedUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserBlock() { }

    private UserBlock(Guid blockerUserId, Guid blockedUserId, DateTime createdAt)
    {
        BlockerUserId = blockerUserId;
        BlockedUserId = blockedUserId;
        CreatedAt = createdAt;
    }

    public static Result<UserBlock> Create(Guid blockerUserId, Guid blockedUserId)
    {
        if (blockerUserId == Guid.Empty)
        {
            return Result.Failure<UserBlock>(new Error("UserBlock.InvalidBlocker", "Blocker is required"));
        }

        if (blockedUserId == Guid.Empty)
        {
            return Result.Failure<UserBlock>(new Error("UserBlock.InvalidBlocked", "Blocked user is required"));
        }

        if (blockerUserId == blockedUserId)
        {
            return Result.Failure<UserBlock>(new Error("UserBlock.SelfBlock", "Cannot block yourself"));
        }

        return Result.Success(new UserBlock(blockerUserId, blockedUserId, DateTime.UtcNow));
    }
}
