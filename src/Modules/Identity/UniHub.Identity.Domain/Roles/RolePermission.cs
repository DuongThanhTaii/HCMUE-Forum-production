using UniHub.Identity.Domain.Permissions;
using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Roles;

public sealed class RolePermission : Entity<Guid>
{
    public RoleId RoleId { get; private set; }
    public PermissionId PermissionId { get; private set; }
    public PermissionScope Scope { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private RolePermission()
    {
        // EF Core constructor
        RoleId = null!;
        PermissionId = null!;
        Scope = null!;
    }

    internal RolePermission(RoleId roleId, PermissionId permissionId, PermissionScope scope)
    {
        Id = Guid.NewGuid();
        RoleId = roleId;
        PermissionId = permissionId;
        Scope = scope;
        AssignedAt = DateTime.UtcNow;
    }

    public bool HasPermissionInScope(PermissionId permissionId, PermissionScope requiredScope)
    {
        return PermissionId == permissionId && Scope.MatchesScope(requiredScope);
    }
}