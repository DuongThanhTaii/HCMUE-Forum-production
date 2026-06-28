using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;

namespace UniHub.Identity.Application.Abstractions;

public interface IGroupPermissionOverrideRepository
{
    Task<List<GroupPermissionOverride>> GetEffectiveByGroupAsync(Guid groupId, DateTime asOfUtc, CancellationToken cancellationToken = default);
    Task<GroupPermissionOverride?> GetByKeyAsync(Guid groupId, PermissionId permissionId, PermissionScope scope, CancellationToken cancellationToken = default);
    Task AddAsync(GroupPermissionOverride overrideItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(GroupPermissionOverride overrideItem, CancellationToken cancellationToken = default);
}
