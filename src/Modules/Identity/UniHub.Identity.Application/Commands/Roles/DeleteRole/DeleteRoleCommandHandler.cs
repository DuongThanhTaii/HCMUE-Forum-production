using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Roles;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Roles.DeleteRole;

/// <summary>
/// Handler for deleting a role
/// </summary>
public sealed class DeleteRoleCommandHandler : ICommandHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    private static readonly string[] SystemRoles = { "Student", "Teacher", "Admin" };

    public DeleteRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        // Get role
        var roleId = new RoleId(request.RoleId);
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(RoleErrors.RoleNotFound);
        }

        // Prevent deletion of system roles
        if (SystemRoles.Contains(role.Name))
        {
            return Result.Failure(RoleErrors.CannotDeleteSystemRole);
        }

        // Delete role
        await _roleRepository.DeleteAsync(role, cancellationToken);

        return Result.Success();
    }
}
