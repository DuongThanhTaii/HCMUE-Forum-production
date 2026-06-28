using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Roles.DeleteRole;

/// <summary>
/// Command to delete a role
/// </summary>
public sealed record DeleteRoleCommand(Guid RoleId) : ICommand;
