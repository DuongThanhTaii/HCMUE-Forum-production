using UniHub.Chat.Domain.Messages;

namespace UniHub.Chat.Domain.Tests.Messages;

public class MessageIdTests
{
    [Fact]
    public void Create_WithValidGuid_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var messageId = MessageId.Create(guid);

        // Assert
        messageId.Should().NotBeNull();
        messageId.Value.Should().Be(guid);
    }

    [Fact]
    public void CreateUnique_ShouldCreateNewGuid()
    {
        // Act
        var messageId = MessageId.CreateUnique();

        // Assert
        messageId.Should().NotBeNull();
        messageId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateUnique_ShouldCreateDifferentIds()
    {
        // Act
        var id1 = MessageId.CreateUnique();
        var id2 = MessageId.CreateUnique();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = MessageId.Create(guid);
        var id2 = MessageId.Create(guid);

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = MessageId.CreateUnique();
        var id2 = MessageId.CreateUnique();

        // Assert
        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHash()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = MessageId.Create(guid);
        var id2 = MessageId.Create(guid);

        // Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldContainGuidValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var messageId = MessageId.Create(guid);

        // Act
        var result = messageId.ToString();

        // Assert
        result.Should().Contain(guid.ToString());
    }
}
