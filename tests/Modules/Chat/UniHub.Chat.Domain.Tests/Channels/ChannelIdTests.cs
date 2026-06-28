using UniHub.Chat.Domain.Channels;

namespace UniHub.Chat.Domain.Tests.Channels;

public class ChannelIdTests
{
    [Fact]
    public void Create_WithValidGuid_ShouldCreateInstance()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var channelId = ChannelId.Create(guid);

        // Assert
        channelId.Should().NotBeNull();
        channelId.Value.Should().Be(guid);
    }

    [Fact]
    public void CreateUnique_ShouldCreateNewGuid()
    {
        // Act
        var channelId = ChannelId.CreateUnique();

        // Assert
        channelId.Should().NotBeNull();
        channelId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void CreateUnique_ShouldCreateDifferentIds()
    {
        // Act
        var id1 = ChannelId.CreateUnique();
        var id2 = ChannelId.CreateUnique();

        // Assert
        id1.Should().NotBe(id2);
        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ChannelId.Create(guid);
        var id2 = ChannelId.Create(guid);

        // Assert
        id1.Should().Be(id2);
        (id1 == id2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var id1 = ChannelId.CreateUnique();
        var id2 = ChannelId.CreateUnique();

        // Assert
        id1.Should().NotBe(id2);
        (id1 == id2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValue_ShouldReturnSameHash()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id1 = ChannelId.Create(guid);
        var id2 = ChannelId.Create(guid);

        // Assert
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    public void ToString_ShouldContainGuidValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var channelId = ChannelId.Create(guid);

        // Act
        var result = channelId.ToString();

        // Assert
        result.Should().Contain(guid.ToString());
    }
}
