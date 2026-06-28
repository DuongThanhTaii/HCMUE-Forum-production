using FluentAssertions;
using UniHub.SharedKernel.Domain;

namespace UniHub.SharedKernel.Tests.Domain;

public class DomainEventHandlerTests
{
    [Fact]
    public async Task DomainEventHandler_ShouldHandleEvent()
    {
        // Arrange
        var handler = new TestDomainEventHandler();
        var domainEvent = new TestDomainEvent { Message = "Test Message" };

        // Act
        await handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        handler.HandledEvents.Should().ContainSingle();
        handler.HandledEvents.First().Message.Should().Be("Test Message");
    }

    [Fact]
    public async Task DomainEventHandler_WithMultipleEvents_ShouldHandleAll()
    {
        // Arrange
        var handler = new TestDomainEventHandler();
        var events = new[]
        {
            new TestDomainEvent { Message = "Event 1" },
            new TestDomainEvent { Message = "Event 2" },
            new TestDomainEvent { Message = "Event 3" }
        };

        // Act
        foreach (var evt in events)
        {
            await handler.Handle(evt, CancellationToken.None);
        }

        // Assert
        handler.HandledEvents.Should().HaveCount(3);
        handler.HandledEvents.Select(e => e.Message).Should().ContainInOrder("Event 1", "Event 2", "Event 3");
    }

    [Fact]
    public async Task DomainEventHandler_WithCancellation_ShouldRespectCancellationToken()
    {
        // Arrange
        var handler = new TestDomainEventHandler();
        var domainEvent = new TestDomainEvent { Message = "Test" };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await handler.Handle(domainEvent, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // Test domain event
    private record TestDomainEvent : IDomainEvent
    {
        public string Message { get; init; } = string.Empty;
    }

    // Test domain event handler
    private class TestDomainEventHandler : IDomainEventHandler<TestDomainEvent>
    {
        public List<TestDomainEvent> HandledEvents { get; } = new();

        public Task Handle(TestDomainEvent notification, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            HandledEvents.Add(notification);
            return Task.CompletedTask;
        }
    }
}
