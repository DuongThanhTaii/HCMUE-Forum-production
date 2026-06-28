namespace UniHub.Learning.Domain.EventSourcing;

/// <summary>
/// Represents a stored domain event with metadata
/// </summary>
public sealed class StoredEvent
{
    /// <summary>
    /// Unique ID của stored event
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// ID của aggregate root
    /// </summary>
    public Guid AggregateId { get; private set; }

    /// <summary>
    /// Loại aggregate (Document, Course, etc.)
    /// </summary>
    public string AggregateType { get; private set; }

    /// <summary>
    /// Tên loại event (DocumentCreatedEvent, DocumentApprovedEvent, etc.)
    /// </summary>
    public string EventType { get; private set; }

    /// <summary>
    /// Version của event trong aggregate stream (sequence number)
    /// </summary>
    public long Version { get; private set; }

    /// <summary>
    /// Serialized event data (JSON)
    /// </summary>
    public string EventData { get; private set; }

    /// <summary>
    /// Thời điểm event xảy ra
    /// </summary>
    public DateTime OccurredOn { get; private set; }

    /// <summary>
    /// Thời điểm event được lưu vào store
    /// </summary>
    public DateTime StoredOn { get; private set; }

    private StoredEvent() 
    {
        AggregateType = string.Empty;
        EventType = string.Empty;
        EventData = string.Empty;
    }

    public StoredEvent(
        Guid aggregateId,
        string aggregateType,
        string eventType,
        long version,
        string eventData,
        DateTime occurredOn)
    {
        Id = Guid.NewGuid();
        AggregateId = aggregateId;
        AggregateType = aggregateType;
        EventType = eventType;
        Version = version;
        EventData = eventData;
        OccurredOn = occurredOn;
        StoredOn = DateTime.UtcNow;
    }

    public static StoredEvent Create(
        Guid aggregateId,
        string aggregateType,
        string eventType,
        long version,
        string eventData,
        DateTime occurredOn)
    {
        return new StoredEvent(aggregateId, aggregateType, eventType, version, eventData, occurredOn);
    }
}
