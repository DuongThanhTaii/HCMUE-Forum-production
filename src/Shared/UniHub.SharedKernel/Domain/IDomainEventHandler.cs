using MediatR;

namespace UniHub.SharedKernel.Domain;

/// <summary>
/// Interface for handling domain events.
/// Extends MediatR.INotificationHandler for async event handling.
/// </summary>
/// <typeparam name="TDomainEvent">The type of domain event to handle.</typeparam>
public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
}
