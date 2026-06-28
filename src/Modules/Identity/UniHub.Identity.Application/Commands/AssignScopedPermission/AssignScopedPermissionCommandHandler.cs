using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Roles;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.AssignScopedPermission;

internal sealed class AssignScopedPermissionCommandHandler : ICommandHandler<AssignScopedPermissionCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionCache _permissionCache;

    public AssignScopedPermissionCommandHandler(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPermissionCache permissionCache)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(AssignScopedPermissionCommand request, CancellationToken cancellationToken)
    {
        // Get the role
        var roleId = new RoleId(request.RoleId);
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        
        if (role is null)
        {
            return Result.Failure(RoleErrors.NotFound);
        }

        // Get the permission
        var permissionId = new PermissionId(request.PermissionId);
        var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
        
        if (permission is null)
        {
            return Result.Failure(PermissionErrors.NotFound);
        }

        // Parse and create the scope
        if (!Enum.TryParse<PermissionScopeType>(request.ScopeType, ignoreCase: true, out var scopeType))
        {
            return Result.Failure(new Error(
                "AssignScopedPermission.InvalidScopeType",
                $"Invalid scope type: {request.ScopeType}"));
        }

        var scopeResult = PermissionScope.Create(scopeType, request.ScopeValue);
        if (scopeResult.IsFailure)
        {
            return Result.Failure(scopeResult.Error);
        }

        // Assign permission with scope
        var assignResult = role.AssignPermission(permissionId, scopeResult.Value);
        if (assignResult.IsFailure)
        {
            return Result.Failure(assignResult.Error);
        }

        // Save the role
        await _roleRepository.UpdateAsync(role, cancellationToken);

        // Invalidate permission cache for all users with this role
        await _permissionCache.InvalidateAllAsync(cancellationToken);

        return Result.Success();
    }
}
