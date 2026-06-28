using MediatR;

namespace UniHub.SharedKernel.Domain;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain that domain experts care about.
/// Extends MediatR.INotification for event dispatching.
/// </summary>
public interface IDomainEvent : INotification
{
}
