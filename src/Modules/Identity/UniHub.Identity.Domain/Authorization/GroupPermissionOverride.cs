using UniHub.Identity.Domain.Permissions;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Authorization;

public sealed class GroupPermissionOverride : Entity<Guid>
{
    public Guid GroupId { get; private set; }
    public PermissionId PermissionId { get; private set; }
    public PermissionScope Scope { get; private set; }
    public PermissionEffect Effect { get; private set; }
    public string? Reason { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public int Version { get; private set; }

    private GroupPermissionOverride()
    {
        PermissionId = null!;
        Scope = null!;
    }

    private GroupPermissionOverride(
        Guid groupId,
        PermissionId permissionId,
        PermissionScope scope,
        PermissionEffect effect,
        string? reason,
        DateTime? expiresAtUtc)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        PermissionId = permissionId;
        Scope = scope;
        Effect = effect;
        Reason = reason;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        Version = 1;
    }

    public static Result<GroupPermissionOverride> Create(
        Guid groupId,
        PermissionId permissionId,
        PermissionScope scope,
        PermissionEffect effect,
        string? reason = null,
        DateTime? expiresAtUtc = null)
    {
        if (groupId == Guid.Empty)
        {
            return Result.Failure<GroupPermissionOverride>(new Error(
                "GroupPermissionOverride.GroupId.Empty",
                "Group id cannot be empty."));
        }

        if (expiresAtUtc.HasValue && expiresAtUtc.Value <= DateTime.UtcNow)
        {
            return Result.Failure<GroupPermissionOverride>(new Error(
                "GroupPermissionOverride.Expiry.Invalid",
                "Expiry time must be in the future."));
        }

        if (reason?.Length > 500)
        {
            return Result.Failure<GroupPermissionOverride>(new Error(
                "GroupPermissionOverride.Reason.TooLong",
                "Reason cannot exceed 500 characters."));
        }

        return Result.Success(new GroupPermissionOverride(
            groupId,
            permissionId,
            scope,
            effect,
            reason?.Trim(),
            expiresAtUtc));
    }

    public bool IsEffectiveAt(DateTime utcNow)
    {
        if (IsRevoked)
        {
            return false;
        }

        return !ExpiresAtUtc.HasValue || ExpiresAtUtc.Value > utcNow;
    }

    public Result Update(
        PermissionEffect effect,
        string? reason,
        DateTime? expiresAtUtc)
    {
        if (expiresAtUtc.HasValue && expiresAtUtc.Value <= DateTime.UtcNow)
        {
            return Result.Failure(new Error(
                "GroupPermissionOverride.Expiry.Invalid",
                "Expiry time must be in the future."));
        }

        if (reason?.Length > 500)
        {
            return Result.Failure(new Error(
                "GroupPermissionOverride.Reason.TooLong",
                "Reason cannot exceed 500 characters."));
        }

        Effect = effect;
        Reason = reason?.Trim();
        ExpiresAtUtc = expiresAtUtc;
        UpdatedAtUtc = DateTime.UtcNow;
        Version++;

        return Result.Success();
    }

    public void Revoke()
    {
        IsRevoked = true;
        UpdatedAtUtc = DateTime.UtcNow;
        Version++;
    }
}
