using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Errors;

public static class RoleErrors
{
    public static readonly Error NotFound = new("Role.NotFound", "Role was not found");
    public static readonly Error NameAlreadyExists = new("Role.NameAlreadyExists", "Role with this name already exists");
    public static readonly Error CannotDeleteSystemRole = new("Role.CannotDeleteSystemRole", "System roles cannot be deleted");
    public static readonly Error CannotDeleteDefaultRole = new("Role.CannotDeleteDefaultRole", "Default role cannot be deleted");
    public static readonly Error CannotDeleteRoleWithUsers = new("Role.CannotDeleteRoleWithUsers", "Cannot delete role that is assigned to users");
    public static readonly Error MultipleDefaultRoles = new("Role.MultipleDefaultRoles", "Only one role can be set as default");
}

public static class PermissionErrors
{
    public static readonly Error NotFound = new("Permission.NotFound", "Permission was not found");
    public static readonly Error CodeAlreadyExists = new("Permission.CodeAlreadyExists", "Permission with this code already exists");
    public static readonly Error InvalidModule = new("Permission.InvalidModule", "Invalid module specified");
}