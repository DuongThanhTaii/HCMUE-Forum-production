using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Queries.GetUserPermissions;

/// <summary>
/// Handler for getting user permissions
/// </summary>
public sealed class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, UserPermissionsResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IPermissionCache _permissionCache;

    public GetUserPermissionsQueryHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IPermissionCache permissionCache)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result<UserPermissionsResponse>> Handle(
        GetUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        // Try to get from cache first
        var cachedPermissions = await _permissionCache.GetUserPermissionsAsync(
            request.UserId,
            cancellationToken);

        if (cachedPermissions is not null)
        {
            return Result.Success(cachedPermissions);
        }

        // Get user with roles
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserPermissionsResponse>(UserErrors.NotFound);
        }

        // Get all roles for the user
        var roleIds = user.Roles.Select(r => r.RoleId).ToList();
        var roles = new List<UniHub.Identity.Domain.Roles.Role>();
        foreach (var roleId in roleIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role is not null)
            {
                roles.Add(role);
            }
        }

        // Collect all unique permissions from all roles
        var permissionIds = roles
            .SelectMany(r => r.Permissions)
            .Select(p => p.PermissionId)
            .Distinct()
            .ToList();

        // Get permission details
        var permissions = new List<PermissionDto>();
        foreach (var permissionId in permissionIds)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            if (permission is not null)
            {
                // Get scope from role permission
                var rolePermission = roles
                    .SelectMany(r => r.Permissions)
                    .FirstOrDefault(p => p.PermissionId == permissionId);

                PermissionScopeDto? scopeDto = null;
                if (rolePermission is not null)
                {
                    scopeDto = new PermissionScopeDto(
                        rolePermission.Scope.Type.ToString(),
                        rolePermission.Scope.Value);
                }

                permissions.Add(new PermissionDto(
                    permission.Id.Value,
                    permission.Code,
                    permission.Name,
                    permission.Description,
                    permission.Module,
                    scopeDto));
            }
        }

        var response = new UserPermissionsResponse(request.UserId, permissions);

        // Cache the result
        await _permissionCache.SetUserPermissionsAsync(
            request.UserId,
            response,
            cancellationToken);

        return Result.Success(response);
    }
}
