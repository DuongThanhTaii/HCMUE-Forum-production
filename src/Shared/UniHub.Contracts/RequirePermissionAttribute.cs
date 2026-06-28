using Microsoft.AspNetCore.Authorization;

namespace UniHub.Contracts;

/// <summary>
/// Attribute that enforces a specific DB permission code via the Permission policy system.
/// Replaces [Authorize(Roles = "...")] for fine-grained, DB-driven authorization (Layer 2).
///
/// Usage:
///   [RequirePermission("identity.roles.create")]
///   [RequirePermission("forum.reports.review")]
///
/// The attribute generates a policy name in the format "Permission:{permissionCode}".
/// This is resolved dynamically by PermissionPolicyProvider in UniHub.Identity.Infrastructure.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Prefix used by PermissionPolicyProvider to identify permission-based policies.
    /// Must match the prefix checked in PermissionPolicyProvider.GetPolicyAsync.
    /// </summary>
    public const string PolicyPrefix = "Permission:";

    /// <summary>
    /// Creates a permission requirement for the given permission code.
    /// </summary>
    /// <param name="permissionCode">
    /// The permission code to check (e.g., "identity.roles.create", "forum.reports.review").
    /// Must match a code in the identity.permissions table.
    /// </param>
    public RequirePermissionAttribute(string permissionCode)
        : base($"{PolicyPrefix}{permissionCode}")
    {
        PermissionCode = permissionCode;
    }

    /// <summary>The permission code being enforced by this attribute.</summary>
    public string PermissionCode { get; }
}
