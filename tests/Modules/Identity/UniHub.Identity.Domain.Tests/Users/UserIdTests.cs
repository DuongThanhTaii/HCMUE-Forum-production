using FluentAssertions;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Domain.Tests.Users;

public class UserIdTests
{
    [Fact]
    public void CreateUnique_ShouldGenerateUniqueId()
    {
        // Act
        var userId1 = UserId.CreateUnique();
        var userId2 = UserId.CreateUnique();

        // Assert
        userId1.Should().NotBe(userId2);
        userId1.Value.Should().NotBeEmpty();
        userId2.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithValidGuid_ShouldCreateUserId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var userId = UserId.Create(guid);

        // Assert
        userId.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId = UserId.Create(guid);

        // Act
        var result = userId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void TwoUserIdsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var userId1 = UserId.Create(guid);
        var userId2 = UserId.Create(guid);

        // Act & Assert
        userId1.Should().Be(userId2);
        (userId1 == userId2).Should().BeTrue();
    }
}