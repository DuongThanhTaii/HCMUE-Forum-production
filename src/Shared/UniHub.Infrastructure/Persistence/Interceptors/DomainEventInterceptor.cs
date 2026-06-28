using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using UniHub.SharedKernel.Domain;

namespace UniHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that collects domain events from aggregate roots and publishes them
/// after changes are saved to the database.
/// </summary>
public sealed class DomainEventInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventInterceptor"/> class.
    /// </summary>
    /// <param name="publisher">The MediatR publisher for dispatching domain events.</param>
    public DomainEventInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    /// <summary>
    /// Called after SaveChanges to publish domain events from aggregate roots.
    /// </summary>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await PublishDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Collects and publishes domain events from all aggregate roots in the context.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    private async Task PublishDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        // Get all aggregate roots that have domain events
        var aggregateRoots = context.ChangeTracker
            .Entries<IHasDomainEvents>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        // Collect all domain events
        var domainEvents = aggregateRoots
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        // Clear domain events from aggregates
        foreach (var aggregate in aggregateRoots)
        {
            aggregate.ClearDomainEvents();
        }

        // Publish all domain events
        foreach (var domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}
