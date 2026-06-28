using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.EventSourcing;

/// <summary>
/// Interface cho Event Store để lưu trữ và truy xuất domain events
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Lưu một domain event vào event store
    /// </summary>
    Task SaveEventAsync<TEvent>(TEvent domainEvent, Guid aggregateId, string aggregateType, CancellationToken cancellationToken = default) 
        where TEvent : IDomainEvent;

    /// <summary>
    /// Lưu nhiều domain events vào event store
    /// </summary>
    Task SaveEventsAsync(IEnumerable<IDomainEvent> events, Guid aggregateId, string aggregateType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả events của một aggregate
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy events của một aggregate từ version cụ thể
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetEventsAsync(Guid aggregateId, long fromVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy tất cả events theo loại aggregate
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetEventsByAggregateTypeAsync(string aggregateType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy events trong khoảng thời gian
    /// </summary>
    Task<IReadOnlyList<StoredEvent>> GetEventsByTimeRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
