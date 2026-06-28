using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Permissions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Authorization.RevokeGroupPermissionOverride;

public sealed class RevokeGroupPermissionOverrideCommandHandler : ICommandHandler<RevokeGroupPermissionOverrideCommand>
{
    private readonly IGroupPermissionOverrideRepository _groupPermissionOverrideRepository;
    private readonly IPermissionCache _permissionCache;

    public RevokeGroupPermissionOverrideCommandHandler(
        IGroupPermissionOverrideRepository groupPermissionOverrideRepository,
        IPermissionCache permissionCache)
    {
        _groupPermissionOverrideRepository = groupPermissionOverrideRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(RevokeGroupPermissionOverrideCommand request, CancellationToken cancellationToken)
    {
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

        var existingOverride = await _groupPermissionOverrideRepository.GetByKeyAsync(
            request.GroupId,
            new PermissionId(request.PermissionId),
            scopeResult.Value,
            cancellationToken);

        if (existingOverride is null || existingOverride.IsRevoked)
        {
            return Result.Failure(new Error(
                "GroupPermissionOverride.NotFound",
                "Group permission override not found."));
        }

        existingOverride.Revoke();
        await _groupPermissionOverrideRepository.UpdateAsync(existingOverride, cancellationToken);
        await _permissionCache.InvalidateAllAsync(cancellationToken);

        return Result.Success();
    }
}
