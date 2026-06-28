using UniHub.Learning.Domain.EventSourcing;

namespace UniHub.Learning.Domain.Tests.EventSourcing;

public class StoredEventTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnStoredEvent()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var aggregateType = "Document";
        var eventType = "DocumentCreatedEvent";
        var version = 1L;
        var eventData = "{\"documentId\":\"test\"}";
        var occurredOn = DateTime.UtcNow;

        // Act
        var storedEvent = StoredEvent.Create(aggregateId, aggregateType, eventType, version, eventData, occurredOn);

        // Assert
        storedEvent.Should().NotBeNull();
        storedEvent.Id.Should().NotBe(Guid.Empty);
        storedEvent.AggregateId.Should().Be(aggregateId);
        storedEvent.AggregateType.Should().Be(aggregateType);
        storedEvent.EventType.Should().Be(eventType);
        storedEvent.Version.Should().Be(version);
        storedEvent.EventData.Should().Be(eventData);
        storedEvent.OccurredOn.Should().Be(occurredOn);
        storedEvent.StoredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_UniqueIds_ShouldGenerateDifferentIds()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        // Act
        var event1 = StoredEvent.Create(aggregateId, "Document", "Event1", 1, "{}", occurredOn);
        var event2 = StoredEvent.Create(aggregateId, "Document", "Event2", 2, "{}", occurredOn);

        // Assert
        event1.Id.Should().NotBe(event2.Id);
    }

    [Fact]
    public void Create_WithSameAggregate_ShouldHaveSameAggregateId()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;

        // Act
        var event1 = StoredEvent.Create(aggregateId, "Document", "Event1", 1, "{}", occurredOn);
        var event2 = StoredEvent.Create(aggregateId, "Document", "Event2", 2, "{}", occurredOn);

        // Assert
        event1.AggregateId.Should().Be(event2.AggregateId);
    }

    [Fact]
    public void StoredEvent_ShouldOrderByVersion()
    {
        // Arrange
        var aggregateId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;
        var events = new List<StoredEvent>
        {
            StoredEvent.Create(aggregateId, "Document", "Event3", 3, "{}", occurredOn),
            StoredEvent.Create(aggregateId, "Document", "Event1", 1, "{}", occurredOn),
            StoredEvent.Create(aggregateId, "Document", "Event2", 2, "{}", occurredOn)
        };

        // Act
        var orderedEvents = events.OrderBy(e => e.Version).ToList();

        // Assert
        orderedEvents[0].Version.Should().Be(1);
        orderedEvents[1].Version.Should().Be(2);
        orderedEvents[2].Version.Should().Be(3);
    }
}
