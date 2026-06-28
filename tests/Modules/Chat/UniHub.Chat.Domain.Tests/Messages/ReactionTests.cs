using UniHub.Chat.Domain.Messages;

namespace UniHub.Chat.Domain.Tests.Messages;

public class ReactionTests
{
    private readonly Guid _userId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var result = Reaction.Create(_userId, "👍");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var reaction = result.Value;
        reaction.UserId.Should().Be(_userId);
        reaction.Emoji.Should().Be("👍");
        reaction.ReactedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithComplexEmoji_ShouldSucceed()
    {
        // Act
        var result = Reaction.Create(_userId, "❤️");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Emoji.Should().Be("❤️");
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldFail()
    {
        // Act
        var result = Reaction.Create(Guid.Empty, "👍");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reaction.InvalidUser");
    }

    [Fact]
    public void Create_WithEmptyEmoji_ShouldFail()
    {
        // Act
        var result = Reaction.Create(_userId, "   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reaction.InvalidEmoji");
    }

    [Fact]
    public void Create_WithEmojiTooLong_ShouldFail()
    {
        // Arrange
        var longEmoji = new string('A', 11);

        // Act
        var result = Reaction.Create(_userId, longEmoji);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reaction.EmojiTooLong");
    }

    [Fact]
    public void Equality_WithSameUserAndEmoji_ShouldBeEqual()
    {
        // Arrange
        var reaction1 = Reaction.Create(_userId, "👍").Value;
        var reaction2 = Reaction.Create(_userId, "👍").Value;

        // Assert
        reaction1.Should().Be(reaction2);
        (reaction1 == reaction2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentUser_ShouldNotBeEqual()
    {
        // Arrange
        var reaction1 = Reaction.Create(_userId, "👍").Value;
        var reaction2 = Reaction.Create(Guid.NewGuid(), "👍").Value;

        // Assert
        reaction1.Should().NotBe(reaction2);
        (reaction1 == reaction2).Should().BeFalse();
    }

    [Fact]
    public void Equality_WithDifferentEmoji_ShouldNotBeEqual()
    {
        // Arrange
        var reaction1 = Reaction.Create(_userId, "👍").Value;
        var reaction2 = Reaction.Create(_userId, "❤️").Value;

        // Assert
        reaction1.Should().NotBe(reaction2);
        (reaction1 == reaction2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHash()
    {
        // Arrange
        var reaction1 = Reaction.Create(_userId, "👍").Value;
        var reaction2 = Reaction.Create(_userId, "👍").Value;

        // Assert
        reaction1.GetHashCode().Should().Be(reaction2.GetHashCode());
    }
}
