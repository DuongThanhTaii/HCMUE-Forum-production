using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Domain.Tests.Posts;

public class PostIdTests
{
    [Fact]
    public void CreateUnique_ShouldGenerateUniqueId()
    {
        // Act
        var postId1 = PostId.CreateUnique();
        var postId2 = PostId.CreateUnique();

        // Assert
        postId1.Should().NotBe(postId2);
        postId1.Value.Should().NotBeEmpty();
        postId2.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithValidGuid_ShouldCreatePostId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var postId = PostId.Create(guid);

        // Assert
        postId.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var postId = PostId.Create(guid);

        // Act
        var result = postId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void TwoPostIdsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var postId1 = PostId.Create(guid);
        var postId2 = PostId.Create(guid);

        // Assert
        postId1.Should().Be(postId2);
    }

    [Fact]
    public void TwoPostIdsWithDifferentValues_ShouldNotBeEqual()
    {
        // Act
        var postId1 = PostId.CreateUnique();
        var postId2 = PostId.CreateUnique();

        // Assert
        postId1.Should().NotBe(postId2);
    }
}
