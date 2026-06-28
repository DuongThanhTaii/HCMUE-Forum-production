using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Application.Abstractions;

public interface IUserPermissionOverrideRepository
{
    Task<List<UserPermissionOverride>> GetEffectiveByUserAsync(UserId userId, DateTime asOfUtc, CancellationToken cancellationToken = default);
    Task<UserPermissionOverride?> GetByKeyAsync(UserId userId, PermissionId permissionId, PermissionScope scope, CancellationToken cancellationToken = default);
    Task AddAsync(UserPermissionOverride overrideItem, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserPermissionOverride overrideItem, CancellationToken cancellationToken = default);
}
