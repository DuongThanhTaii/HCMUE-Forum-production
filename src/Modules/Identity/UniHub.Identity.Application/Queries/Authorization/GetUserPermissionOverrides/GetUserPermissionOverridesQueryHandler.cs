using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Authorization;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Queries.Authorization.GetUserPermissionOverrides;

public sealed class GetUserPermissionOverridesQueryHandler
    : IQueryHandler<GetUserPermissionOverridesQuery, IReadOnlyList<PermissionOverrideItemResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUserPermissionOverrideRepository _userPermissionOverrideRepository;
    private readonly IPermissionRepository _permissionRepository;

    public GetUserPermissionOverridesQueryHandler(
        IUserRepository userRepository,
        IUserPermissionOverrideRepository userPermissionOverrideRepository,
        IPermissionRepository permissionRepository)
    {
        _userRepository = userRepository;
        _userPermissionOverrideRepository = userPermissionOverrideRepository;
        _permissionRepository = permissionRepository;
    }

    public async Task<Result<IReadOnlyList<PermissionOverrideItemResponse>>> Handle(
        GetUserPermissionOverridesQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(new UserId(request.UserId), cancellationToken);
        if (user is null)
        {
            return Result.Failure<IReadOnlyList<PermissionOverrideItemResponse>>(
                new Error("User.NotFound", "User not found."));
        }

        var asOfUtc = request.AsOfUtc ?? DateTime.UtcNow;
        var overrides = await _userPermissionOverrideRepository.GetEffectiveByUserAsync(
            user.Id,
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
