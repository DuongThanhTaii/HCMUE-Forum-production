using UniHub.Identity.Domain.Authorization;

namespace UniHub.Identity.Application.Abstractions;

public interface IEndpointToggleRepository
{
    Task<EndpointToggle?> GetByEndpointKeyAsync(string endpointKey, CancellationToken cancellationToken = default);
    Task<List<EndpointToggle>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(EndpointToggle endpointToggle, CancellationToken cancellationToken = default);
    Task UpdateAsync(EndpointToggle endpointToggle, CancellationToken cancellationToken = default);
}
