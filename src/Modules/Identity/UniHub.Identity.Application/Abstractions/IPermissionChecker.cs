using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Application.Abstractions;

/// <summary>
/// Service for checking user permissions with scope awareness
/// </summary>
public interface IPermissionChecker
{
    /// <summary>
    /// Checks if a user has a specific permission globally
    /// </summary>
    Task<bool> HasPermissionAsync(UserId userId, string permissionCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific permission in a given scope
    /// </summary>
    Task<bool> HasPermissionInScopeAsync(
        UserId userId, 
        string permissionCode, 
        PermissionScope scope, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions globally
    /// </summary>
    Task<bool> HasAnyPermissionAsync(
        UserId userId, 
        IEnumerable<string> permissionCodes, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions in a given scope
    /// </summary>
    Task<bool> HasAnyPermissionInScopeAsync(
        UserId userId, 
        IEnumerable<string> permissionCodes, 
        PermissionScope scope, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has all of the specified permissions globally
    /// </summary>
    Task<bool> HasAllPermissionsAsync(
        UserId userId, 
        IEnumerable<string> permissionCodes, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has all of the specified permissions in a given scope
    /// </summary>
    Task<bool> HasAllPermissionsInScopeAsync(
        UserId userId, 
        IEnumerable<string> permissionCodes, 
        PermissionScope scope, 
        CancellationToken cancellationToken = default);
}
