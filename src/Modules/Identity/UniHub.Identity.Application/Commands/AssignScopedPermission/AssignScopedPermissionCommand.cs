using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.AssignScopedPermission;

/// <summary>
/// Command to assign a permission with a specific scope to a role
/// </summary>
public sealed record AssignScopedPermissionCommand(
    Guid RoleId,
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue = null) : ICommand;
