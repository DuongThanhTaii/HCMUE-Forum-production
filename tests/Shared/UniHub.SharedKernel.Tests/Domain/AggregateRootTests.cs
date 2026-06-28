using FluentAssertions;
using UniHub.SharedKernel.Domain;

namespace UniHub.SharedKernel.Tests.Domain;

public class AggregateRootTests
{
    private record TestId : GuidId
    {
        public TestId(Guid value) : base(value) { }
        public TestId() : base() { }
    }

    private record TestDomainEvent(string Message) : IDomainEvent;

    private class TestAggregateRoot : AggregateRoot<TestId>
    {
        public string Name { get; private set; } = string.Empty;

        public TestAggregateRoot() { }

        public TestAggregateRoot(TestId id, string name) : base(id)
        {
            Name = name;
        }

        public void ChangeName(string newName)
        {
            Name = newName;
            AddDomainEvent(new TestDomainEvent($"Name changed to {newName}"));
        }

        public void DoSomething()
        {
            AddDomainEvent(new TestDomainEvent("Something happened"));
        }
    }

    [Fact]
    public void AggregateRoot_WhenCreated_ShouldHaveNoDomainEvents()
    {
        // Arrange & Act
        var aggregate = new TestAggregateRoot(new TestId(), "Test");

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(new TestId(), "Test");

        // Act
        aggregate.ChangeName("New Name");

        // Assert
        aggregate.DomainEvents.Should().HaveCount(1);
        aggregate.DomainEvents.First().Should().BeOfType<TestDomainEvent>();
        ((TestDomainEvent)aggregate.DomainEvents.First()).Message.Should().Be("Name changed to New Name");
    }

    [Fact]
    public void AddDomainEvent_Multiple_ShouldAddAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(new TestId(), "Test");

        // Act
        aggregate.ChangeName("Name 1");
        aggregate.ChangeName("Name 2");
        aggregate.DoSomething();

        // Assert
        aggregate.DomainEvents.Should().HaveCount(3);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(new TestId(), "Test");
        aggregate.ChangeName("New Name");
        aggregate.DoSomething();
        aggregate.DomainEvents.Should().HaveCount(2);

        // Act
        aggregate.ClearDomainEvents();

        // Assert
        aggregate.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var aggregate = new TestAggregateRoot(new TestId(), "Test");

        // Act
        aggregate.ChangeName("New Name");

        // Assert
        aggregate.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }
}
