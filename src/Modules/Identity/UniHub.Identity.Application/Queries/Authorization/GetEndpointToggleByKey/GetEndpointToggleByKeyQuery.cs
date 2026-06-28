using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Queries.Authorization.GetEndpointToggleByKey;

public sealed record GetEndpointToggleByKeyQuery(string EndpointKey) : IQuery<EndpointToggleItemResponse>;
