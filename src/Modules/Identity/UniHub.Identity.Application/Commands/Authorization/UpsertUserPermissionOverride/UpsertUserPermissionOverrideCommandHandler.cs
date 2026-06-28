using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Authorization.UpsertUserPermissionOverride;

public sealed class UpsertUserPermissionOverrideCommandHandler : ICommandHandler<UpsertUserPermissionOverrideCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserPermissionOverrideRepository _userPermissionOverrideRepository;
    private readonly IPermissionCache _permissionCache;

    public UpsertUserPermissionOverrideCommandHandler(
        IUserRepository userRepository,
        IPermissionRepository permissionRepository,
        IUserPermissionOverrideRepository userPermissionOverrideRepository,
        IPermissionCache permissionCache)
    {
        _userRepository = userRepository;
        _permissionRepository = permissionRepository;
        _userPermissionOverrideRepository = userPermissionOverrideRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(UpsertUserPermissionOverrideCommand request, CancellationToken cancellationToken)
    {
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
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
                "UserPermissionOverride.InvalidScopeType",
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
                "UserPermissionOverride.InvalidEffect",
                $"Invalid effect: {request.Effect}"));
        }

        var scope = scopeResult.Value;
        var existingOverride = await _userPermissionOverrideRepository.GetByKeyAsync(
            userId,
            permissionId,
            scope,
            cancellationToken);

        if (existingOverride is null || existingOverride.IsRevoked)
        {
            var createResult = UserPermissionOverride.Create(
                userId,
                permissionId,
                scope,
                effect,
                request.Reason,
                request.ExpiresAtUtc);

            if (createResult.IsFailure)
            {
                return Result.Failure(createResult.Error);
            }

            await _userPermissionOverrideRepository.AddAsync(createResult.Value, cancellationToken);
        }
        else
        {
            var updateResult = existingOverride.Update(effect, request.Reason, request.ExpiresAtUtc);
            if (updateResult.IsFailure)
            {
                return Result.Failure(updateResult.Error);
            }

            await _userPermissionOverrideRepository.UpdateAsync(existingOverride, cancellationToken);
        }

        await _permissionCache.InvalidateUserPermissionsAsync(request.UserId, cancellationToken);

        return Result.Success();
    }
}
