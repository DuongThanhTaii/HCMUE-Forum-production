using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Users.AssignRole;

/// <summary>
/// Command to assign a role to a user
/// </summary>
public sealed record AssignRoleCommand(
    Guid UserId,
    Guid RoleId) : ICommand;
