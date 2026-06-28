using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Queries.Authorization.GetEndpointToggles;

public sealed record GetEndpointTogglesQuery : IQuery<IReadOnlyList<EndpointToggleItemResponse>>;
