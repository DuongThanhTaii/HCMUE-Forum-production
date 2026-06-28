using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Roles.UpdateRole;

/// <summary>
/// Command to update an existing role
/// </summary>
public sealed record UpdateRoleCommand(
    Guid RoleId,
    string Name,
    string Description) : ICommand;
