namespace UniHub.SharedKernel.Domain;

/// <summary>
/// Interface for entities that can raise domain events.
/// Typically implemented by aggregate roots.
/// </summary>
public interface IHasDomainEvents
{
    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    /// <summary>
    /// Clears all domain events from this entity.
    /// Should be called after events have been dispatched.
    /// </summary>
    void ClearDomainEvents();
}
