using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.RemoveScopedPermission;

/// <summary>
/// Command to remove a permission with a specific scope from a role
/// </summary>
public sealed record RemoveScopedPermissionCommand(
    Guid RoleId,
    Guid PermissionId,
    string ScopeType,
    string? ScopeValue = null) : ICommand;
