using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

public sealed class GroupPermissionOverrideRepository : IGroupPermissionOverrideRepository
{
    private static readonly List<GroupPermissionOverride> Overrides = new();
    private static readonly object LockObj = new();

    public Task<List<GroupPermissionOverride>> GetEffectiveByGroupAsync(Guid groupId, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            var result = Overrides
                .Where(item => item.GroupId == groupId && item.IsEffectiveAt(asOfUtc))
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<GroupPermissionOverride?> GetByKeyAsync(Guid groupId, PermissionId permissionId, PermissionScope scope, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            var result = Overrides.FirstOrDefault(item =>
                item.GroupId == groupId &&
                item.PermissionId == permissionId &&
                item.Scope == scope);

            return Task.FromResult(result);
        }
    }

    public Task AddAsync(GroupPermissionOverride overrideItem, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            Overrides.Add(overrideItem);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(GroupPermissionOverride overrideItem, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
