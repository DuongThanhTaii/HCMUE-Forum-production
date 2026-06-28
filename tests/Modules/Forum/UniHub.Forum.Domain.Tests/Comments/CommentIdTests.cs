using UniHub.Forum.Domain.Comments;

namespace UniHub.Forum.Domain.Tests.Comments;

public class CommentIdTests
{
    [Fact]
    public void CreateUnique_ShouldGenerateUniqueId()
    {
        // Act
        var commentId1 = CommentId.CreateUnique();
        var commentId2 = CommentId.CreateUnique();

        // Assert
        commentId1.Should().NotBe(commentId2);
        commentId1.Value.Should().NotBeEmpty();
        commentId2.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithValidGuid_ShouldCreateCommentId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var commentId = CommentId.Create(guid);

        // Assert
        commentId.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var commentId = CommentId.Create(guid);

        // Act
        var result = commentId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void TwoCommentIdsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var commentId1 = CommentId.Create(guid);
        var commentId2 = CommentId.Create(guid);

        // Assert
        commentId1.Should().Be(commentId2);
    }

    [Fact]
    public void TwoCommentIdsWithDifferentValues_ShouldNotBeEqual()
    {
        // Act
        var commentId1 = CommentId.CreateUnique();
        var commentId2 = CommentId.CreateUnique();

        // Assert
        commentId1.Should().NotBe(commentId2);
    }
}
