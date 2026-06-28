using System.Text.Json;
using UniHub.Learning.Domain.Documents.Events;
using UniHub.Learning.Domain.EventSourcing;

namespace UniHub.Learning.Domain.Tests.EventSourcing;

public class EventSourcingHelperTests
{
    [Fact]
    public void SerializeEvent_WithDomainEvent_ShouldReturnJson()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var uploaderId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;
        var domainEvent = new DocumentCreatedEvent(documentId, uploaderId, "Slide", occurredOn);

        // Act
        var json = EventSourcingHelper.SerializeEvent(domainEvent);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("documentId");
        json.Should().Contain("uploaderId");
        json.Should().Contain(documentId.ToString());
    }

    [Fact]
    public void SerializeEvent_ShouldUseCamelCase()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var uploaderId = Guid.NewGuid();
        var occurredOn = DateTime.UtcNow;
        var domainEvent = new DocumentCreatedEvent(documentId, uploaderId, "Slide", occurredOn);

        // Act
        var json = EventSourcingHelper.SerializeEvent(domainEvent);

        // Assert
        json.Should().Contain("\"documentId\"");
        json.Should().NotContain("\"DocumentId\"");
    }

    [Fact]
    public void GetEventTypeName_FromType_ShouldReturnFullName()
    {
        // Act
        var typeName = EventSourcingHelper.GetEventTypeName<DocumentCreatedEvent>();

        // Assert
        typeName.Should().NotBeNullOrEmpty();
        typeName.Should().Contain("DocumentCreatedEvent");
    }

    [Fact]
    public void GetEventTypeName_FromInstance_ShouldReturnFullName()
    {
        // Arrange
        var domainEvent = new DocumentCreatedEvent(Guid.NewGuid(), Guid.NewGuid(), "Slide", DateTime.UtcNow);

        // Act
        var typeName = EventSourcingHelper.GetEventTypeName(domainEvent);

        // Assert
        typeName.Should().NotBeNullOrEmpty();
        typeName.Should().Contain("DocumentCreatedEvent");
    }

    [Fact]
    public void SerializeAndDeserialize_ShouldPreserveEventData()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();
        var comment = "Test approval comment";
        var occurredOn = DateTime.UtcNow;
        var originalEvent = new DocumentApprovedEvent(documentId, reviewerId, comment, occurredOn);
        var eventTypeName = EventSourcingHelper.GetEventTypeName(originalEvent);

        // Act
        var json = EventSourcingHelper.SerializeEvent(originalEvent);
        var deserializedEvent = EventSourcingHelper.DeserializeEvent(json, eventTypeName) as DocumentApprovedEvent;

        // Assert
        deserializedEvent.Should().NotBeNull();
        deserializedEvent!.DocumentId.Should().Be(documentId);
        deserializedEvent.ApproverId.Should().Be(reviewerId);
        deserializedEvent.ApprovalComment.Should().Be(comment);
        deserializedEvent.OccurredOn.Should().BeCloseTo(occurredOn, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void DeserializeEvent_WithInvalidType_ShouldThrowException()
    {
        // Arrange
        var json = "{}";
        var invalidType = "NonExistentEventType";

        // Act & Assert
        Action act = () => EventSourcingHelper.DeserializeEvent(json, invalidType);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SerializeEvent_WithComplexEvent_ShouldSerializeAllProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var scanResult = "Flagged";
        var confidence = 0.75;
        var flaggedReasons = new List<string> { "Reason1", "Reason2" };
        var occurredOn = DateTime.UtcNow;
        var domainEvent = new DocumentAIScannedEvent(documentId, scanResult, confidence, flaggedReasons, occurredOn);

        // Act
        var json = EventSourcingHelper.SerializeEvent(domainEvent);

        // Assert
        json.Should().Contain("\"scanResult\"");
        json.Should().Contain("\"confidence\"");
        json.Should().Contain("\"flaggedReasons\"");
        json.Should().Contain("Reason1");
        json.Should().Contain("Reason2");
    }
}
