using FluentAssertions;
using UniHub.Infrastructure.Caching;

namespace UniHub.Infrastructure.Tests.Caching;

public class CacheKeysTests
{
    [Fact]
    public void User_ShouldReturnCorrectKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var key = CacheKeys.User(userId);

        // Assert
        key.Should().Be($"user:{userId}");
    }

    [Fact]
    public void Post_ShouldReturnCorrectKey()
    {
        // Arrange
        var postId = Guid.NewGuid();

        // Act
        var key = CacheKeys.Post(postId);

        // Assert
        key.Should().Be($"post:{postId}");
    }

    [Fact]
    public void Forum_ShouldReturnCorrectKey()
    {
        // Arrange
        var forumId = Guid.NewGuid();

        // Act
        var key = CacheKeys.Forum(forumId);

        // Assert
        key.Should().Be($"forum:{forumId}");
    }

    [Fact]
    public void UserNotifications_ShouldReturnCorrectKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var key = CacheKeys.UserNotifications(userId);

        // Assert
        key.Should().Be($"notification:{userId}");
    }

    [Fact]
    public void Course_ShouldReturnCorrectKey()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        // Act
        var key = CacheKeys.Course(courseId);

        // Assert
        key.Should().Be($"course:{courseId}");
    }

    [Fact]
    public void UserChats_ShouldReturnCorrectKey()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var key = CacheKeys.UserChats(userId);

        // Assert
        key.Should().Be($"chat:{userId}");
    }

    [Fact]
    public void Pattern_ShouldReturnCorrectPattern()
    {
        // Arrange
        var prefix = "user:";

        // Act
        var pattern = CacheKeys.Pattern(prefix);

        // Assert
        pattern.Should().Be("user:*");
    }

    [Fact]
    public void Prefixes_ShouldHaveCorrectValues()
    {
        // Assert
        CacheKeys.UserPrefix.Should().Be("user:");
        CacheKeys.PostPrefix.Should().Be("post:");
        CacheKeys.ForumPrefix.Should().Be("forum:");
        CacheKeys.NotificationPrefix.Should().Be("notification:");
        CacheKeys.CoursePrefix.Should().Be("course:");
        CacheKeys.ChatPrefix.Should().Be("chat:");
    }
}
