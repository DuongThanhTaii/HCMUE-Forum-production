using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Roles;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Roles.UpdateRole;

/// <summary>
/// Handler for updating an existing role
/// </summary>
public sealed class UpdateRoleCommandHandler : ICommandHandler<UpdateRoleCommand>
{
    private readonly IRoleRepository _roleRepository;

    public UpdateRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // Get role
        var roleId = new RoleId(request.RoleId);
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(RoleErrors.RoleNotFound);
        }

        // Check if new name already exists (if changed)
        if (role.Name != request.Name)
        {
            var existingRole = await _roleRepository.GetByNameAsync(request.Name, cancellationToken);
            if (existingRole is not null)
            {
                return Result.Failure(RoleErrors.RoleAlreadyExists);
            }
        }

        // Update role name
        var updateNameResult = role.UpdateName(request.Name);
        if (updateNameResult.IsFailure)
        {
            return updateNameResult;
        }

        // Update role description
        var updateDescResult = role.UpdateDescription(request.Description);
        if (updateDescResult.IsFailure)
        {
            return updateDescResult;
        }

        // Save role
        await _roleRepository.UpdateAsync(role, cancellationToken);

        return Result.Success();
    }
}
