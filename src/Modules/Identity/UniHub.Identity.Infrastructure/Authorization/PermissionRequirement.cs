namespace UniHub.Identity.Infrastructure.Authorization;

/// <summary>
/// Represents a permission requirement for ASP.NET Core authorization policies.
/// </summary>
/// <param name="PermissionCode">The permission code (e.g., "identity.roles.create")</param>
public sealed record PermissionRequirement(string PermissionCode)
    : Microsoft.AspNetCore.Authorization.IAuthorizationRequirement;
