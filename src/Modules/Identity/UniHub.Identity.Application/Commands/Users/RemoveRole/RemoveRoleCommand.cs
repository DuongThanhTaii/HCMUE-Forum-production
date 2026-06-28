using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Users.RemoveRole;

/// <summary>
/// Command to remove a role from a user
/// </summary>
public sealed record RemoveRoleCommand(
    Guid UserId,
    Guid RoleId) : ICommand;
