using UniHub.Identity.Application.Queries.GetUserPermissions;

namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// Interface for caching user permissions
/// </summary>
public interface IPermissionCache
{
    /// <summary>
    /// Gets cached permissions for a user
    /// </summary>
    Task<UserPermissionsResponse?> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Caches permissions for a user
    /// </summary>
    Task SetUserPermissionsAsync(
        Guid userId,
        UserPermissionsResponse permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates cached permissions for a user
    /// </summary>
    Task InvalidateUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cached permissions
    /// </summary>
    Task InvalidateAllAsync(CancellationToken cancellationToken = default);
}
