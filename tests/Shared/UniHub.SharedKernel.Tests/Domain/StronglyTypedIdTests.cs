using FluentAssertions;
using UniHub.SharedKernel.Domain;

namespace UniHub.SharedKernel.Tests.Domain;

public class StronglyTypedIdTests
{
    private record UserId : GuidId
    {
        public UserId(Guid value) : base(value) { }
        public UserId() : base() { }
    }

    private record ProductId : IntId
    {
        public ProductId(int value) : base(value) { }
    }

    private record OrderCode : StringId
    {
        public OrderCode(string value) : base(value) { }
    }

    [Fact]
    public void GuidId_WhenCreatedWithValue_ShouldHaveThatValue()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var userId = new UserId(guid);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void GuidId_WhenCreatedWithoutValue_ShouldGenerateNewGuid()
    {
        // Act
        var userId1 = new UserId();
        var userId2 = new UserId();

        // Assert
        userId1.Value.Should().NotBe(Guid.Empty);
        userId2.Value.Should().NotBe(Guid.Empty);
        userId1.Value.Should().NotBe(userId2.Value);
    }

    [Fact]
    public void IntId_WhenCreated_ShouldHaveCorrectValue()
    {
        // Arrange & Act
        var productId = new ProductId(42);

        // Assert
        productId.Value.Should().Be(42);
    }

    [Fact]
    public void StringId_WhenCreatedWithValidValue_ShouldHaveThatValue()
    {
        // Arrange & Act
        var orderCode = new OrderCode("ORD-12345");

        // Assert
        orderCode.Value.Should().Be("ORD-12345");
    }

    [Fact]
    public void StringId_WhenCreatedWithEmptyString_ShouldThrowException()
    {
        // Act
        Action act = () => new OrderCode(string.Empty);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("String ID cannot be null or whitespace.*");
    }

    [Fact]
    public void StringId_WhenCreatedWithWhitespace_ShouldThrowException()
    {
        // Act
        Action act = () => new OrderCode("   ");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("String ID cannot be null or whitespace.*");
    }

    [Fact]
    public void StronglyTypedIds_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = new UserId(guid);
        var userId2 = new UserId(guid);

        // Act & Assert
        userId1.Should().Be(userId2);
        userId1.Equals(userId2).Should().BeTrue();
    }

    [Fact]
    public void StronglyTypedIds_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var userId1 = new UserId(Guid.NewGuid());
        var userId2 = new UserId(Guid.NewGuid());

        // Act & Assert
        userId1.Should().NotBe(userId2);
    }

    [Fact]
    public void StronglyTypedId_ToString_ShouldReturnValueAsString()
    {
        // Arrange
        var productId = new ProductId(42);

        // Act
        var result = productId.ToString();

        // Assert
        result.Should().Contain("42");
    }
}
