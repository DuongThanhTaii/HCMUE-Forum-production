using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Queries.Authorization.GetEndpointToggleByKey;

public sealed class GetEndpointToggleByKeyQueryHandler
    : IQueryHandler<GetEndpointToggleByKeyQuery, EndpointToggleItemResponse>
{
    private readonly IEndpointToggleRepository _endpointToggleRepository;

    public GetEndpointToggleByKeyQueryHandler(IEndpointToggleRepository endpointToggleRepository)
    {
        _endpointToggleRepository = endpointToggleRepository;
    }

    public async Task<Result<EndpointToggleItemResponse>> Handle(
        GetEndpointToggleByKeyQuery request,
        CancellationToken cancellationToken)
    {
        var endpointKey = request.EndpointKey.Trim();
        var toggle = await _endpointToggleRepository.GetByEndpointKeyAsync(endpointKey, cancellationToken);
        if (toggle is null)
        {
            return Result.Failure<EndpointToggleItemResponse>(new Error(
                "EndpointToggle.NotFound",
                "Endpoint toggle not found."));
        }

        return Result.Success(new EndpointToggleItemResponse(
            toggle.EndpointKey,
            toggle.IsEnabled,
            toggle.Reason,
            toggle.UpdatedBy,
            toggle.UpdatedAtUtc,
            toggle.Version));
    }
}
