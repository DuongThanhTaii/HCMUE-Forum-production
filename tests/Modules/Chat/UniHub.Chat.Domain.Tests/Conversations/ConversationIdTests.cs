using UniHub.Chat.Domain.Conversations;

namespace UniHub.Chat.Domain.Tests.Conversations;

public class ConversationIdTests
{
    [Fact]
    public void Create_WithValidGuid_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var conversationId = ConversationId.Create(guid);

        // Assert
        conversationId.Should().NotBeNull();
        conversationId.Value.Should().Be(guid);
    }

    [Fact]
    public void CreateUnique_ShouldCreateNewGuid()
    {
        // Act
        var conversationId = ConversationId.CreateUnique();

        // Assert
        conversationId.Should().NotBeNull();
        conversationId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateUnique_ShouldCreateDifferentIds()
    {
        // Act
        var id1 = ConversationId.CreateUnique();
        var id2 = ConversationId.CreateUnique();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ConversationId.Create(guid);
        var id2 = ConversationId.Create(guid);

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = ConversationId.CreateUnique();
        var id2 = ConversationId.CreateUnique();

        // Assert
        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHash()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ConversationId.Create(guid);
        var id2 = ConversationId.Create(guid);

        // Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldContainGuidValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var conversationId = ConversationId.Create(guid);

        // Act
        var result = conversationId.ToString();

        // Assert
        result.Should().Contain(guid.ToString());
    }
}
