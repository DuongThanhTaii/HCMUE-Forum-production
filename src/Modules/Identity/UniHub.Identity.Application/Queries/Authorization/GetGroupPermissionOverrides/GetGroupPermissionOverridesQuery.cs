using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Queries.Authorization.GetGroupPermissionOverrides;

public sealed record GetGroupPermissionOverridesQuery(
    Guid GroupId,
    DateTime? AsOfUtc = null) : IQuery<IReadOnlyList<PermissionOverrideItemResponse>>;
