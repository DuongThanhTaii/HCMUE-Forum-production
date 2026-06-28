using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Commands.Roles.CreateRole;

/// <summary>
/// Command to create a new role
/// </summary>
public sealed record CreateRoleCommand(
    string Name,
    string Description) : ICommand<Guid>;
