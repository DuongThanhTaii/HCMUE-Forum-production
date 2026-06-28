using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Roles;

/// <summary>
/// Error codes for role operations
/// </summary>
public static class RoleErrors
{
    public static Error RoleAlreadyExists => new Error(
        "Role.AlreadyExists",
        "A role with this name already exists");

    public static Error RoleNotFound => new Error(
        "Role.NotFound",
        "Role not found");

    public static Error CannotDeleteSystemRole => new Error(
        "Role.CannotDeleteSystem",
        "Cannot delete system roles (Student, Teacher, Admin)");

    public static Error PermissionNotFound => new Error(
        "Role.PermissionNotFound",
        "Permission not found");

    public static Error PermissionAlreadyAssigned => new Error(
        "Role.PermissionAlreadyAssigned",
        "Permission is already assigned to this role");

    public static Error PermissionNotAssigned => new Error(
        "Role.PermissionNotAssigned",
        "Permission is not assigned to this role");
}
