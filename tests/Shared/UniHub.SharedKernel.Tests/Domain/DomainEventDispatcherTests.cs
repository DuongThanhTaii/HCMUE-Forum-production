using FluentAssertions;
using MediatR;
using Moq;
using UniHub.SharedKernel.Domain;

namespace UniHub.SharedKernel.Tests.Domain;

public class DomainEventDispatcherTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly DomainEventDispatcher _dispatcher;

    public DomainEventDispatcherTests()
    {
        _publisherMock = new Mock<IPublisher>();
        _dispatcher = new DomainEventDispatcher(_publisherMock.Object);
    }

    [Fact]
    public async Task DispatchAsync_WithSingleEvent_ShouldPublishEvent()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();
        var publishCalled = false;
        
        _publisherMock
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => publishCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        await _dispatcher.DispatchAsync(domainEvent);

        // Assert
        publishCalled.Should().BeTrue();
    }

    [Fact]
    public async Task DispatchAsync_WithMultipleEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent(),
            new TestDomainEvent(),
            new TestDomainEvent()
        };
        var publishCount = 0;

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => publishCount++)
            .Returns(Task.CompletedTask);

        // Act
        await _dispatcher.DispatchAsync(events);

        // Assert
        publishCount.Should().Be(3);
    }

    [Fact]
    public async Task DispatchAsync_WithEmptyCollection_ShouldNotPublish()
    {
        // Arrange
        var events = new List<IDomainEvent>();
        var publishCalled = false;

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => publishCalled = true)
            .Returns(Task.CompletedTask);

        // Act
        await _dispatcher.DispatchAsync(events);

        // Assert
        publishCalled.Should().BeFalse();
    }

    [Fact]
    public async Task DispatchAsync_WithCancellationToken_ShouldPassTokenToPublisher()
    {
        // Arrange
        var domainEvent = new TestDomainEvent();
        var cancellationToken = new CancellationToken();
        CancellationToken receivedToken = default;

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((_, ct) => receivedToken = ct)
            .Returns(Task.CompletedTask);

        // Act
        await _dispatcher.DispatchAsync(domainEvent, cancellationToken);

        // Assert
        receivedToken.Should().Be(cancellationToken);
    }

    [Fact]
    public async Task DispatchAsync_MultipleEvents_ShouldDispatchSequentially()
    {
        // Arrange
        var dispatchOrder = new List<int>();
        var events = new List<IDomainEvent>
        {
            new TestDomainEvent { Order = 1 },
            new TestDomainEvent { Order = 2 },
            new TestDomainEvent { Order = 3 }
        };

        _publisherMock
            .Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()))
            .Callback<object, CancellationToken>((evt, ct) =>
            {
                if (evt is TestDomainEvent testEvent)
                {
                    dispatchOrder.Add(testEvent.Order);
                }
            });

        // Act
        await _dispatcher.DispatchAsync(events);

        // Assert
        dispatchOrder.Should().ContainInOrder(1, 2, 3);
    }

    // Test domain event
    private record TestDomainEvent : IDomainEvent
    {
        public int Order { get; init; }
    }
}
