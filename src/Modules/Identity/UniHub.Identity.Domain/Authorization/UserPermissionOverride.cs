using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Authorization;

public sealed class UserPermissionOverride : Entity<Guid>
{
    public UserId UserId { get; private set; }
    public PermissionId PermissionId { get; private set; }
    public PermissionScope Scope { get; private set; }
    public PermissionEffect Effect { get; private set; }
    public string? Reason { get; private set; }
    public DateTime? ExpiresAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public int Version { get; private set; }

    private UserPermissionOverride()
    {
        UserId = null!;
        PermissionId = null!;
        Scope = null!;
    }

    private UserPermissionOverride(
        UserId userId,
        PermissionId permissionId,
        PermissionScope scope,
        PermissionEffect effect,
        string? reason,
        DateTime? expiresAtUtc)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PermissionId = permissionId;
        Scope = scope;
        Effect = effect;
        Reason = reason;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTime.UtcNow;
        Version = 1;
    }

    public static Result<UserPermissionOverride> Create(
        UserId userId,
        PermissionId permissionId,
        PermissionScope scope,
        PermissionEffect effect,
        string? reason = null,
        DateTime? expiresAtUtc = null)
    {
        if (expiresAtUtc.HasValue && expiresAtUtc.Value <= DateTime.UtcNow)
        {
            return Result.Failure<UserPermissionOverride>(new Error(
                "UserPermissionOverride.Expiry.Invalid",
                "Expiry time must be in the future."));
        }

        if (reason?.Length > 500)
        {
            return Result.Failure<UserPermissionOverride>(new Error(
                "UserPermissionOverride.Reason.TooLong",
                "Reason cannot exceed 500 characters."));
        }

        return Result.Success(new UserPermissionOverride(
            userId,
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
                "UserPermissionOverride.Expiry.Invalid",
                "Expiry time must be in the future."));
        }

        if (reason?.Length > 500)
        {
            return Result.Failure(new Error(
                "UserPermissionOverride.Reason.TooLong",
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
