using UniHub.Identity.Domain.Permissions;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Roles;

public sealed class Role : AggregateRoot<RoleId>
{
    private readonly List<RolePermission> _permissions = new();

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsSystemRole { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<RolePermission> Permissions => _permissions.AsReadOnly();

    private Role()
    {
        // EF Core constructor
        Name = string.Empty;
    }

    private Role(RoleId id, string name, string? description, bool isDefault, bool isSystemRole)
    {
        Id = id;
        Name = name;
        Description = description;
        IsDefault = isDefault;
        IsSystemRole = isSystemRole;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Role> Create(string name, string? description = null, bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Role>(new Error("Role.Name.Empty", "Role name cannot be empty"));
        }

        if (name.Trim().Length > 100)
        {
            return Result.Failure<Role>(new Error("Role.Name.TooLong", "Role name cannot exceed 100 characters"));
        }

        if (description?.Length > 500)
        {
            return Result.Failure<Role>(new Error("Role.Description.TooLong", "Role description cannot exceed 500 characters"));
        }

        return Result.Success(new Role(
            RoleId.CreateUnique(),
            name.Trim(),
            description?.Trim(),
            isDefault,
            isSystemRole: false));
    }

    public static Result<Role> CreateSystem(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Role>(new Error("Role.Name.Empty", "Role name cannot be empty"));
        }

        return Result.Success(new Role(
            RoleId.CreateUnique(),
            name.Trim(),
            description?.Trim(),
            isDefault: false,
            isSystemRole: true));
    }

    public Result UpdateName(string newName)
    {
        if (IsSystemRole)
        {
            return Result.Failure(new Error("Role.SystemRole.CannotUpdate", "System roles cannot be updated"));
        }

        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure(new Error("Role.Name.Empty", "Role name cannot be empty"));
        }

        if (newName.Trim().Length > 100)
        {
            return Result.Failure(new Error("Role.Name.TooLong", "Role name cannot exceed 100 characters"));
        }

        Name = newName.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateDescription(string? newDescription)
    {
        if (IsSystemRole)
        {
            return Result.Failure(new Error("Role.SystemRole.CannotUpdate", "System roles cannot be updated"));
        }

        if (newDescription?.Length > 500)
        {
            return Result.Failure(new Error("Role.Description.TooLong", "Role description cannot exceed 500 characters"));
        }

        Description = newDescription?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AssignPermission(PermissionId permissionId, PermissionScope scope)
    {
        if (IsSystemRole)
        {
            return Result.Failure(new Error("Role.SystemRole.CannotModify", "System roles cannot be modified"));
        }

        if (_permissions.Any(p => p.PermissionId == permissionId && p.Scope.Equals(scope)))
        {
            return Result.Failure(new Error("Role.Permission.AlreadyAssigned", "Permission with this scope is already assigned to role"));
        }

        _permissions.Add(new RolePermission(Id, permissionId, scope));
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemovePermission(PermissionId permissionId, PermissionScope scope)
    {
        if (IsSystemRole)
        {
            return Result.Failure(new Error("Role.SystemRole.CannotModify", "System roles cannot be modified"));
        }

        var rolePermission = _permissions.FirstOrDefault(p => 
            p.PermissionId == permissionId && p.Scope.Equals(scope));

        if (rolePermission is null)
        {
            return Result.Failure(new Error("Role.Permission.NotAssigned", "Permission with this scope is not assigned to role"));
        }

        _permissions.Remove(rolePermission);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveAllPermissions()
    {
        if (IsSystemRole)
        {
            return Result.Failure(new Error("Role.SystemRole.CannotModify", "System roles cannot be modified"));
        }

        _permissions.Clear();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public bool HasPermission(PermissionId permissionId, PermissionScope requiredScope)
    {
        return _permissions.Any(p => p.HasPermissionInScope(permissionId, requiredScope));
    }

    public bool HasAnyPermissionForModule(string module)
    {
        // This would require joining with Permission entity to check the module
        // Implementation would depend on how you access Permission details
        return _permissions.Any();
    }

    public Result SetAsDefault()
    {
        // Only Student role among system roles can be default
        if (IsSystemRole && Name != "Student")
        {
            return Result.Failure(new Error("Role.SystemRole.CannotSetDefault", "System roles cannot be set as default"));
        }

        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveAsDefault()
    {
        IsDefault = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }
}