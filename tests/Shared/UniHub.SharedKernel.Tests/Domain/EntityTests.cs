using FluentAssertions;
using UniHub.SharedKernel.Domain;

namespace UniHub.SharedKernel.Tests.Domain;

public class EntityTests
{
    private record TestId : GuidId
    {
        public TestId(Guid value) : base(value) { }
        public TestId() : base() { }
    }

    private class TestEntity : Entity<TestId>
    {
        public string Name { get; set; } = string.Empty;

        public TestEntity() { }
        public TestEntity(TestId id, string name) : base(id)
        {
            Name = name;
        }
    }

    [Fact]
    public void Entities_WithSameId_ShouldBeEqual()
    {
        // Arrange
        var id = new TestId();
        var entity1 = new TestEntity(id, "Entity 1");
        var entity2 = new TestEntity(id, "Entity 2"); // Different name, same ID

        // Act & Assert
        entity1.Should().Be(entity2);
        (entity1 == entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeTrue();
    }

    [Fact]
    public void Entities_WithDifferentIds_ShouldNotBeEqual()
    {
        // Arrange
        var entity1 = new TestEntity(new TestId(), "Entity 1");
        var entity2 = new TestEntity(new TestId(), "Entity 1"); // Same name, different IDs

        // Act & Assert
        entity1.Should().NotBe(entity2);
        (entity1 != entity2).Should().BeTrue();
        entity1.Equals(entity2).Should().BeFalse();
    }

    [Fact]
    public void Entities_WithSameId_ShouldHaveSameHashCode()
    {
        // Arrange
        var id = new TestId();
        var entity1 = new TestEntity(id, "Entity 1");
        var entity2 = new TestEntity(id, "Entity 2");

        // Act & Assert
        entity1.GetHashCode().Should().Be(entity2.GetHashCode());
    }

    [Fact]
    public void Entity_ComparedToNull_ShouldNotBeEqual()
    {
        // Arrange
        var entity = new TestEntity(new TestId(), "Entity");

        // Act & Assert
        entity.Equals(null).Should().BeFalse();
        (entity == null).Should().BeFalse();
        (entity != null).Should().BeTrue();
    }
}
