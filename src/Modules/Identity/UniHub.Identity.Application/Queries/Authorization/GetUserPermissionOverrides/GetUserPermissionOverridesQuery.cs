using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Queries.Authorization.GetUserPermissionOverrides;

public sealed record GetUserPermissionOverridesQuery(
    Guid UserId,
    DateTime? AsOfUtc = null) : IQuery<IReadOnlyList<PermissionOverrideItemResponse>>;
