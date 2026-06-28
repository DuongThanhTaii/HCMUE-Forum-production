using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Queries.Authorization.GetGroupPermissionOverrides;

public sealed class GetGroupPermissionOverridesQueryHandler
    : IQueryHandler<GetGroupPermissionOverridesQuery, IReadOnlyList<PermissionOverrideItemResponse>>
{
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IGroupPermissionOverrideRepository _groupPermissionOverrideRepository;
    private readonly IPermissionRepository _permissionRepository;

    public GetGroupPermissionOverridesQueryHandler(
        IUserGroupRepository userGroupRepository,
        IGroupPermissionOverrideRepository groupPermissionOverrideRepository,
        IPermissionRepository permissionRepository)
    {
        _userGroupRepository = userGroupRepository;
        _groupPermissionOverrideRepository = groupPermissionOverrideRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<IReadOnlyList<PermissionOverrideItemResponse>>> Handle(
        GetGroupPermissionOverridesQuery request,
        CancellationToken cancellationToken)
    {
        var group = await _userGroupRepository.GetByIdAsync(request.GroupId, cancellationToken);
        if (group is null)
        {
            return Result.Failure<IReadOnlyList<PermissionOverrideItemResponse>>(
                new Error("UserGroup.NotFound", "User group not found."));
        }

        var asOfUtc = request.AsOfUtc ?? DateTime.UtcNow;
        var overrides = await _groupPermissionOverrideRepository.GetEffectiveByGroupAsync(
            request.GroupId,
            asOfUtc,
            cancellationToken);

        var results = new List<PermissionOverrideItemResponse>(overrides.Count);

        foreach (var overrideItem in overrides)
        {
            var permission = await _permissionRepository.GetByIdAsync(overrideItem.PermissionId, cancellationToken);

            results.Add(new PermissionOverrideItemResponse(
                overrideItem.Id,
                overrideItem.PermissionId.Value,
                permission?.Code ?? string.Empty,
                overrideItem.Scope.Type.ToString(),
                overrideItem.Scope.Value,
                overrideItem.Effect.ToString(),
                overrideItem.Reason,
                overrideItem.ExpiresAtUtc,
                overrideItem.CreatedAtUtc,
                overrideItem.UpdatedAtUtc,
                overrideItem.IsRevoked));
        }

        return Result.Success<IReadOnlyList<PermissionOverrideItemResponse>>(results);
    }
}
