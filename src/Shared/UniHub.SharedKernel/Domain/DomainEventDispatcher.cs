using MediatR;

namespace UniHub.SharedKernel.Domain;

/// <summary>
/// Service responsible for dispatching domain events.
/// Uses MediatR to publish events to all registered handlers.
/// </summary>
public class DomainEventDispatcher
{
    private readonly IPublisher _publisher;

    public DomainEventDispatcher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <summary>
    /// Dispatches a single domain event to all registered handlers.
    /// </summary>
    /// <param name="domainEvent">The domain event to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        CancellationToken cancellationToken = default)
    {
        await _publisher.Publish(domainEvent, cancellationToken);
    }

    /// <summary>
    /// Dispatches multiple domain events to all registered handlers.
    /// Events are dispatched sequentially in the order they are provided.
    /// </summary>
    /// <param name="domainEvents">Collection of domain events to dispatch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
