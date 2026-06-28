using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using UniHub.Infrastructure.Persistence.Interceptors;
using UniHub.SharedKernel.Domain;

namespace UniHub.Infrastructure.Tests.Persistence.Interceptors;

public class DomainEventInterceptorTests
{
    [Fact]
    public async Task SaveChangesAsync_WhenAggregateHasDomainEvents_ShouldPublishEvents()
    {
        // Arrange
        var publisherMock = new Mock<IPublisher>();
        var (context, aggregate) = CreateContextAndAggregate(publisherMock.Object);
        
        var domainEvent = new TestDomainEvent();
        aggregate.AddTestDomainEvent(domainEvent);
        
        context.Set<TestAggregateRoot>().Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        publisherMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAggregateHasMultipleDomainEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var publisherMock = new Mock<IPublisher>();
        var (context, aggregate) = CreateContextAndAggregate(publisherMock.Object);
        
        aggregate.AddTestDomainEvent(new TestDomainEvent());
        aggregate.AddTestDomainEvent(new TestDomainEvent());
        aggregate.AddTestDomainEvent(new TestDomainEvent());
        
        context.Set<TestAggregateRoot>().Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        publisherMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task SaveChangesAsync_WhenMultipleAggregatesHaveDomainEvents_ShouldPublishAllEvents()
    {
        // Arrange
        var publisherMock = new Mock<IPublisher>();
        var context = CreateTestDbContext(publisherMock.Object);
        
        var aggregate1 = new TestAggregateRoot(1) { Name = "Aggregate 1" };
        var aggregate2 = new TestAggregateRoot(2) { Name = "Aggregate 2" };
        
        aggregate1.AddTestDomainEvent(new TestDomainEvent());
        aggregate2.AddTestDomainEvent(new TestDomainEvent());
        
        context.Set<TestAggregateRoot>().AddRange(aggregate1, aggregate2);

        // Act
        await context.SaveChangesAsync();

        // Assert
        publisherMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task SaveChangesAsync_AfterPublishing_ShouldClearDomainEvents()
    {
        // Arrange
        var publisherMock = new Mock<IPublisher>();
        var (context, aggregate) = CreateContextAndAggregate(publisherMock.Object);
        
        aggregate.AddTestDomainEvent(new TestDomainEvent());
        context.Set<TestAggregateRoot>().Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task SaveChangesAsync_WhenNoAggregatesHaveDomainEvents_ShouldNotPublishEvents()
    {
        // Arrange
        var publisherMock = new Mock<IPublisher>();
        var (context, aggregate) = CreateContextAndAggregate(publisherMock.Object);
        
        context.Set<TestAggregateRoot>().Add(aggregate);

        // Act
        await context.SaveChangesAsync();

        // Assert
        publisherMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_WhenAggregateModified_ShouldPublishEvents()
    {
        // Arrange
        var publisherMock = new Mock<IPublisher>();
        var (context, aggregate) = CreateContextAndAggregate(publisherMock.Object);
        
        context.Set<TestAggregateRoot>().Add(aggregate);
        await context.SaveChangesAsync();
        publisherMock.Reset();

        // Act
        aggregate.Name = "Updated Name";
        aggregate.AddTestDomainEvent(new TestDomainEvent());
        context.Set<TestAggregateRoot>().Update(aggregate);
        await context.SaveChangesAsync();

        // Assert
        publisherMock.Verify(
            x => x.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static (TestDbContext context, TestAggregateRoot aggregate) CreateContextAndAggregate(IPublisher publisher)
    {
        var context = CreateTestDbContext(publisher);
        var aggregate = new TestAggregateRoot(1) { Name = "Test Aggregate" };
        return (context, aggregate);
    }

    private static TestDbContext CreateTestDbContext(IPublisher publisher)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .AddInterceptors(new DomainEventInterceptor(publisher))
            .Options;

        return new TestDbContext(options);
    }

    // Test DbContext
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestAggregateRoot> Aggregates => Set<TestAggregateRoot>();
    }

    // Test aggregate root
    private class TestAggregateRoot : AggregateRoot<int>
    {
        public string Name { get; set; } = string.Empty;

        public TestAggregateRoot() : base() { }
        public TestAggregateRoot(int id) : base(id) { }

        public void AddTestDomainEvent(IDomainEvent domainEvent)
        {
            AddDomainEvent(domainEvent);
        }
    }

    // Test domain event
    public record TestDomainEvent : IDomainEvent;
}
