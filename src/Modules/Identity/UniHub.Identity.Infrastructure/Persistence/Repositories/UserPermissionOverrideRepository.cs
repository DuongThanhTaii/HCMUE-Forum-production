using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserPermissionOverrideRepository : IUserPermissionOverrideRepository
{
    private static readonly List<UserPermissionOverride> Overrides = new();
    private static readonly object LockObj = new();

    public Task<List<UserPermissionOverride>> GetEffectiveByUserAsync(UserId userId, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            var result = Overrides
                .Where(item => item.UserId == userId && item.IsEffectiveAt(asOfUtc))
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<UserPermissionOverride?> GetByKeyAsync(UserId userId, PermissionId permissionId, PermissionScope scope, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            var result = Overrides.FirstOrDefault(item =>
                item.UserId == userId &&
                item.PermissionId == permissionId &&
                item.Scope == scope);

            return Task.FromResult(result);
        }
    }

    public Task AddAsync(UserPermissionOverride overrideItem, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            Overrides.Add(overrideItem);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserPermissionOverride overrideItem, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
