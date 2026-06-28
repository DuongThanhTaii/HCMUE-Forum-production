using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Authorization.RevokeUserPermissionOverride;

public sealed class RevokeUserPermissionOverrideCommandHandler : ICommandHandler<RevokeUserPermissionOverrideCommand>
{
    private readonly IUserPermissionOverrideRepository _userPermissionOverrideRepository;
    private readonly IPermissionCache _permissionCache;

    public RevokeUserPermissionOverrideCommandHandler(
        IUserPermissionOverrideRepository userPermissionOverrideRepository,
        IPermissionCache permissionCache)
    {
        _userPermissionOverrideRepository = userPermissionOverrideRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(RevokeUserPermissionOverrideCommand request, CancellationToken cancellationToken)
    {
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

        var existingOverride = await _userPermissionOverrideRepository.GetByKeyAsync(
            new UserId(request.UserId),
            new PermissionId(request.PermissionId),
            scopeResult.Value,
            cancellationToken);

        if (existingOverride is null || existingOverride.IsRevoked)
        {
            return Result.Failure(new Error(
                "UserPermissionOverride.NotFound",
                "User permission override not found."));
        }

        existingOverride.Revoke();
        await _userPermissionOverrideRepository.UpdateAsync(existingOverride, cancellationToken);
        await _permissionCache.InvalidateUserPermissionsAsync(request.UserId, cancellationToken);

        return Result.Success();
    }
}
