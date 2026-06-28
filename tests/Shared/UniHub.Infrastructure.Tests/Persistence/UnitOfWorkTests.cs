using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using UniHub.Infrastructure.Persistence;
using UniHub.SharedKernel.Domain;

namespace UniHub.Infrastructure.Tests.Persistence;

public class UnitOfWorkTests
{
    private readonly Mock<IPublisher> _publisherMock;
    private readonly TestDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new TestDbContext(options);
        _publisherMock = new Mock<IPublisher>();
        _unitOfWork = new UnitOfWork(_context, _publisherMock.Object);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldSaveChangesToDatabase()
    {
        // Arrange
        var entity = new TestEntity { Name = "Test" };
        _context.TestEntities.Add(entity);

        // Act
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        _context.TestEntities.Should().ContainSingle();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldDispatchDomainEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var domainEvent = new TestDomainEvent();
        aggregate.AddTestEvent(domainEvent);

        _context.TestAggregates.Add(aggregate);

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        _publisherMock.Verify(
            x => x.Publish(
                It.IsAny<IDomainEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldClearDomainEventsAfterDispatching()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        aggregate.AddTestEvent(new TestDomainEvent());

        _context.TestAggregates.Add(aggregate);

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldStartNewTransaction()
    {
        // Act
        var act = async () => await _unitOfWork.BeginTransactionAsync();

        // Assert - InMemory database ignores transactions but shouldn't throw
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task BeginTransactionAsync_WhenTransactionExists_ShouldThrowException()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();

        // Act
        var act = async () => await _unitOfWork.BeginTransactionAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("A transaction is already in progress.");
    }

    [Fact]
    public async Task CommitTransactionAsync_ShouldCommitTransaction()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();
        var entity = new TestEntity { Name = "Test" };
        _context.TestEntities.Add(entity);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var act = async () => await _unitOfWork.CommitTransactionAsync();

        // Assert - InMemory database ignores transactions but shouldn't throw
        await act.Should().NotThrowAsync();
        _context.TestEntities.Should().ContainSingle();
    }

    [Fact]
    public async Task CommitTransactionAsync_WhenNoTransaction_ShouldThrowException()
    {
        // Act
        var act = async () => await _unitOfWork.CommitTransactionAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No transaction is in progress.");
    }

    [Fact]
    public async Task RollbackTransactionAsync_ShouldRollbackTransaction()
    {
        // Arrange
        await _unitOfWork.BeginTransactionAsync();
        var entity = new TestEntity { Name = "Test" };
        _context.TestEntities.Add(entity);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var act = async () => await _unitOfWork.RollbackTransactionAsync();

        // Assert - InMemory database ignores transactions but shouldn't throw
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RollbackTransactionAsync_WhenNoTransaction_ShouldThrowException()
    {
        // Act
        var act = async () => await _unitOfWork.RollbackTransactionAsync();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No transaction is in progress.");
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleDomainEvents_ShouldDispatchAll()
    {
        // Arrange
        var aggregate = new TestAggregateRoot();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        
        aggregate.AddTestEvent(event1);
        aggregate.AddTestEvent(event2);

        _context.TestAggregates.Add(aggregate);

        // Act
        await _unitOfWork.SaveChangesAsync();

        // Assert
        _publisherMock.Verify(
            x => x.Publish(
                It.IsAny<IDomainEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    // Test classes
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities { get; set; } = null!;
        public DbSet<TestAggregateRoot> TestAggregates { get; set; } = null!;
    }

    public class TestEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class TestAggregateRoot : IHasDomainEvents
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        public int Id { get; set; }
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddTestEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }

    public class TestDomainEvent : IDomainEvent
    {
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
