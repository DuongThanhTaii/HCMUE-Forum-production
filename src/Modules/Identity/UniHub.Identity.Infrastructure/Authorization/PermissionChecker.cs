using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Queries.GetUserPermissions;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Infrastructure.Authorization;

internal sealed class PermissionChecker : IPermissionChecker
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IUserPermissionOverrideRepository _userPermissionOverrideRepository;
    private readonly IGroupPermissionOverrideRepository _groupPermissionOverrideRepository;
    private readonly IPermissionCache _permissionCache;

    public PermissionChecker(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IUserGroupRepository userGroupRepository,
        IUserPermissionOverrideRepository userPermissionOverrideRepository,
        IGroupPermissionOverrideRepository groupPermissionOverrideRepository,
        IPermissionCache permissionCache)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _userGroupRepository = userGroupRepository;
        _userPermissionOverrideRepository = userPermissionOverrideRepository;
        _groupPermissionOverrideRepository = groupPermissionOverrideRepository;
        _permissionCache = permissionCache;
    }

    public async Task<bool> HasPermissionAsync(
        UserId userId,
        string permissionCode,
        CancellationToken cancellationToken = default)
    {
        return await HasPermissionInScopeAsync(
            userId,
            permissionCode,
            PermissionScope.Global(),
            cancellationToken);
    }

    public async Task<bool> HasPermissionInScopeAsync(
        UserId userId,
        string permissionCode,
        PermissionScope scope,
        CancellationToken cancellationToken = default)
    {
        var normalizedPermissionCode = permissionCode.Trim().ToLowerInvariant();
        var permission = await _permissionRepository.GetByCodeAsync(normalizedPermissionCode, cancellationToken);
        if (permission is null)
        {
            return false;
        }

        var overrideDecision = await EvaluateOverridesAsync(
            userId,
            permission.Id,
            scope,
            cancellationToken);

        if (overrideDecision.HasValue)
        {
            return overrideDecision.Value;
        }

        // Try to get from cache first
        var cachedPermissions = await _permissionCache.GetUserPermissionsAsync(userId.Value, cancellationToken);

        if (cachedPermissions != null)
        {
            return CheckPermissionInCache(cachedPermissions, normalizedPermissionCode, scope);
        }

        // Cache miss - load from repository
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        var userPermissions = await GetUserPermissionsWithScopesAsync(user, cancellationToken);

        // Cache the result
        await _permissionCache.SetUserPermissionsAsync(userId.Value, userPermissions, cancellationToken);

        return CheckPermissionInList(userPermissions, normalizedPermissionCode, scope);
    }

    private async Task<bool?> EvaluateOverridesAsync(
        UserId userId,
        PermissionId permissionId,
        PermissionScope requiredScope,
        CancellationToken cancellationToken)
    {
        var utcNow = DateTime.UtcNow;

        var userOverrides = await _userPermissionOverrideRepository.GetEffectiveByUserAsync(
            userId,
            utcNow,
            cancellationToken);

        var matchingUserOverrides = userOverrides
            .Where(item => item.PermissionId == permissionId && item.Scope.MatchesScope(requiredScope))
            .ToList();

        if (matchingUserOverrides.Any(item => item.Effect == PermissionEffect.Deny))
        {
            return false;
        }

        if (matchingUserOverrides.Any(item => item.Effect == PermissionEffect.Allow))
        {
            return true;
        }

        var groups = await _userGroupRepository.GetByMemberAsync(userId, cancellationToken);
        var hasAllowFromGroup = false;

        foreach (var group in groups)
        {
            var groupOverrides = await _groupPermissionOverrideRepository.GetEffectiveByGroupAsync(
                group.Id,
                utcNow,
                cancellationToken);

            var matchingGroupOverrides = groupOverrides
                .Where(item => item.PermissionId == permissionId && item.Scope.MatchesScope(requiredScope))
                .ToList();

            if (matchingGroupOverrides.Any(item => item.Effect == PermissionEffect.Deny))
            {
                return false;
            }

            if (matchingGroupOverrides.Any(item => item.Effect == PermissionEffect.Allow))
            {
                hasAllowFromGroup = true;
            }
        }

        if (hasAllowFromGroup)
        {
            return true;
        }

        return null;
    }

    public async Task<bool> HasAnyPermissionAsync(
        UserId userId,
        IEnumerable<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        return await HasAnyPermissionInScopeAsync(
            userId,
            permissionCodes,
            PermissionScope.Global(),
            cancellationToken);
    }

    public async Task<bool> HasAnyPermissionInScopeAsync(
        UserId userId,
        IEnumerable<string> permissionCodes,
        PermissionScope scope,
        CancellationToken cancellationToken = default)
    {
        var codes = permissionCodes.ToList();
        if (!codes.Any())
        {
            return false;
        }

        foreach (var code in codes)
        {
            if (await HasPermissionInScopeAsync(userId, code, scope, cancellationToken))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<bool> HasAllPermissionsAsync(
        UserId userId,
        IEnumerable<string> permissionCodes,
        CancellationToken cancellationToken = default)
    {
        return await HasAllPermissionsInScopeAsync(
            userId,
            permissionCodes,
            PermissionScope.Global(),
            cancellationToken);
    }

    public async Task<bool> HasAllPermissionsInScopeAsync(
        UserId userId,
        IEnumerable<string> permissionCodes,
        PermissionScope scope,
        CancellationToken cancellationToken = default)
    {
        var codes = permissionCodes.ToList();
        if (!codes.Any())
        {
            return false;
        }

        foreach (var code in codes)
        {
            if (!await HasPermissionInScopeAsync(userId, code, scope, cancellationToken))
            {
                return false;
            }
        }

        return true;
    }

    private async Task<UserPermissionsResponse> GetUserPermissionsWithScopesAsync(
        User user,
        CancellationToken cancellationToken)
    {
        var permissionDtos = new List<PermissionDto>();

        // Get all roles for the user
        var roleIds = user.Roles.Select(ur => ur.RoleId).ToList();

        foreach (var roleId in roleIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role is null) continue;

            // Get all permissions with their scopes for this role
            foreach (var rolePermission in role.Permissions)
            {
                var permission = await _permissionRepository.GetByIdAsync(
                    rolePermission.PermissionId,
                    cancellationToken);

                if (permission is not null)
                {
                    var scopeDto = rolePermission.Scope.IsGlobal
                        ? null
                        : new PermissionScopeDto(
                            rolePermission.Scope.Type.ToString(),
                            rolePermission.Scope.Value);

                    permissionDtos.Add(new PermissionDto(
                        permission.Id.Value,
                        permission.Code,
                        permission.Name,
                        permission.Description,
                        permission.Module,
                        scopeDto));
                }
            }
        }

        return new UserPermissionsResponse(user.Id.Value, permissionDtos);
    }

    private static bool CheckPermissionInCache(
        UserPermissionsResponse cachedPermissions,
        string permissionCode,
        PermissionScope requiredScope)
    {
        return cachedPermissions.Permissions.Any(p =>
            p.Code == permissionCode &&
            MatchesScope(p.Scope, requiredScope));
    }

    private static bool CheckPermissionInList(
        UserPermissionsResponse permissions,
        string permissionCode,
        PermissionScope requiredScope)
    {
        return permissions.Permissions.Any(p =>
            p.Code == permissionCode &&
            MatchesScope(p.Scope, requiredScope));
    }

    private static bool MatchesScope(PermissionScopeDto? permissionScope, PermissionScope requiredScope)
    {
        // If permission has no scope (global), it matches any required scope
        if (permissionScope is null)
        {
            return true;
        }

        // If required scope is global, it matches any permission scope
        if (requiredScope.IsGlobal)
        {
            return true;
        }

        // Both have scopes - must match type and value
        return permissionScope.Type == requiredScope.Type.ToString() &&
               permissionScope.Value == requiredScope.Value;
    }
}
