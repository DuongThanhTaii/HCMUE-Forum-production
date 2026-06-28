using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Queries.Authorization.GetEndpointToggles;

public sealed class GetEndpointTogglesQueryHandler
    : IQueryHandler<GetEndpointTogglesQuery, IReadOnlyList<EndpointToggleItemResponse>>
{
    private readonly IEndpointToggleRepository _endpointToggleRepository;

    public GetEndpointTogglesQueryHandler(IEndpointToggleRepository endpointToggleRepository)
    {
        _endpointToggleRepository = endpointToggleRepository;
    }

    public async Task<Result<IReadOnlyList<EndpointToggleItemResponse>>> Handle(
        GetEndpointTogglesQuery request,
        CancellationToken cancellationToken)
    {
        var toggles = await _endpointToggleRepository.GetAllAsync(cancellationToken);

        var responses = toggles
            .OrderBy(item => item.EndpointKey)
            .Select(item => new EndpointToggleItemResponse(
                item.EndpointKey,
                item.IsEnabled,
                item.Reason,
                item.UpdatedBy,
                item.UpdatedAtUtc,
                item.Version))
            .ToList();

        return Result.Success<IReadOnlyList<EndpointToggleItemResponse>>(responses);
    }
}
