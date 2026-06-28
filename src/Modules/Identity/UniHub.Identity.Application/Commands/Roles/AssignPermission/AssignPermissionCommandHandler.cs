using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Roles;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Roles.AssignPermission;

/// <summary>
/// Handler for assigning a permission to a role
/// </summary>
public sealed class AssignPermissionCommandHandler : ICommandHandler<AssignPermissionCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;

    public AssignPermissionCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<Result> Handle(AssignPermissionCommand request, CancellationToken cancellationToken)
    {
        // Get role
        var roleId = new RoleId(request.RoleId);
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(RoleErrors.RoleNotFound);
        }

        // Get permission
        var permissionId = new PermissionId(request.PermissionId);
        var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Failure(RoleErrors.PermissionNotFound);
        }

        // Create permission scope
        var scopeResult = PermissionScope.Create(request.ScopeType, request.ScopeValue);
        if (scopeResult.IsFailure)
        {
            return Result.Failure(scopeResult.Error);
        }

        // Assign permission to role
        var assignResult = role.AssignPermission(permissionId, scopeResult.Value);
        if (assignResult.IsFailure)
        {
            return assignResult;
        }

        // Save role
        await _roleRepository.UpdateAsync(role, cancellationToken);

        return Result.Success();
    }
}
