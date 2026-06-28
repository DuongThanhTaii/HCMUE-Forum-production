using UniHub.Identity.Domain.Permissions;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Roles.AssignPermission;

/// <summary>
/// Command to assign a permission to a role
/// </summary>
public sealed record AssignPermissionCommand(
    Guid RoleId,
    Guid PermissionId,
    PermissionScopeType ScopeType,
    string? ScopeValue = null) : ICommand;
