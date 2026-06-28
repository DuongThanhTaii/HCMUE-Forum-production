using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using UniHub.Contracts;

namespace UniHub.Identity.Infrastructure.Authorization;

/// <summary>
/// Dynamically creates authorization policies for permission-based attributes.
/// Policy name format: "Permission:{permissionCode}"
/// e.g., "Permission:identity.roles.create"
/// </summary>
public sealed class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(RequirePermissionAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            // Fall back to default policies (e.g., role-based, Authenticated)
            return await base.GetPolicyAsync(policyName);
        }

        var permissionCode = policyName[RequirePermissionAttribute.PolicyPrefix.Length..];

        return new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permissionCode))
            .Build();
    }
}
