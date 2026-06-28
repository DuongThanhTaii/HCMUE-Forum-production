using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Permissions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Authorization.UpsertGroupPermissionOverride;

public sealed class UpsertGroupPermissionOverrideCommandHandler : ICommandHandler<UpsertGroupPermissionOverrideCommand>
{
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IGroupPermissionOverrideRepository _groupPermissionOverrideRepository;
    private readonly IPermissionCache _permissionCache;

    public UpsertGroupPermissionOverrideCommandHandler(
        IUserGroupRepository userGroupRepository,
        IPermissionRepository permissionRepository,
        IGroupPermissionOverrideRepository groupPermissionOverrideRepository,
        IPermissionCache permissionCache)
    {
        _userGroupRepository = userGroupRepository;
        _permissionRepository = permissionRepository;
        _groupPermissionOverrideRepository = groupPermissionOverrideRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(UpsertGroupPermissionOverrideCommand request, CancellationToken cancellationToken)
    {
        var group = await _userGroupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
        {
            return Result.Failure(new Error(
                "UserGroup.NotFound",
                "User group not found."));
        }

        var permissionId = new PermissionId(request.PermissionId);
        var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Failure(PermissionErrors.NotFound);
        }

        if (!Enum.TryParse<PermissionScopeType>(request.ScopeType, ignoreCase: true, out var scopeType))
        {
            return Result.Failure(new Error(
                "GroupPermissionOverride.InvalidScopeType",
                $"Invalid scope type: {request.ScopeType}"));
        }

        var scopeResult = PermissionScope.Create(scopeType, request.ScopeValue);
        if (scopeResult.IsFailure)
        {
            return Result.Failure(scopeResult.Error);
        }

        if (!Enum.TryParse<PermissionEffect>(request.Effect, ignoreCase: true, out var effect))
        {
            return Result.Failure(new Error(
                "GroupPermissionOverride.InvalidEffect",
                $"Invalid effect: {request.Effect}"));
        }

        var scope = scopeResult.Value;
        var existingOverride = await _groupPermissionOverrideRepository.GetByKeyAsync(
            request.GroupId,
            permissionId,
            scope,
            cancellationToken);

        if (existingOverride is null || existingOverride.IsRevoked)
        {
            var createResult = GroupPermissionOverride.Create(
                request.GroupId,
                permissionId,
                scope,
                effect,
                request.Reason,
                request.ExpiresAtUtc);

            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }

            await _groupPermissionOverrideRepository.AddAsync(createResult.Value, cancellationToken);
        }
        else
        {
            var updateResult = existingOverride.Update(effect, request.Reason, request.ExpiresAtUtc);
            if (updateResult.IsFailure)
            {
                return Result.Failure(updateResult.Error);
            }

            await _groupPermissionOverrideRepository.UpdateAsync(existingOverride, cancellationToken);
        }

        await _permissionCache.InvalidateAllAsync(cancellationToken);
        return Result.Success();
    }
}
