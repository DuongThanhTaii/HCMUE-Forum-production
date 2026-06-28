using UniHub.Identity.Domain.Permissions;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Roles.RemovePermission;

/// <summary>
/// Command to remove a permission from a role
/// </summary>
public sealed record RemovePermissionCommand(
    Guid RoleId,
    Guid PermissionId,
    PermissionScopeType ScopeType,
    string? ScopeValue = null) : ICommand;
