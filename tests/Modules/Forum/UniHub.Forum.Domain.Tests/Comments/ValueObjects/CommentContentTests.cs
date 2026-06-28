using UniHub.Forum.Domain.Comments.ValueObjects;

namespace UniHub.Forum.Domain.Tests.Comments.ValueObjects;

public class CommentContentTests
{
    [Fact]
    public void Create_WithValidContent_ShouldReturnSuccess()
    {
        // Arrange
        var contentValue = "This is a valid comment";

        // Act
        var result = CommentContent.Create(contentValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(contentValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceContent_ShouldReturnFailure(string? contentValue)
    {
        // Act
        var result = CommentContent.Create(contentValue!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CommentContent.Empty");
    }

    [Fact]
    public void Create_WithContentTooLong_ShouldReturnFailure()
    {
        // Arrange
        var contentValue = new string('a', CommentContent.MaxLength + 1);

        // Act
        var result = CommentContent.Create(contentValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CommentContent.TooLong");
    }

    [Fact]
    public void Create_WithContentAtMinLength_ShouldReturnSuccess()
    {
        // Arrange
        var contentValue = new string('a', CommentContent.MinLength);

        // Act
        var result = CommentContent.Create(contentValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithContentAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var contentValue = new string('a', CommentContent.MaxLength);

        // Act
        var result = CommentContent.Create(contentValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        var contentValue = "  Valid comment content  ";

        // Act
        var result = CommentContent.Create(contentValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid comment content");
    }

    [Fact]
    public void TwoCommentContentsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var content1 = CommentContent.Create("Same content").Value;
        var content2 = CommentContent.Create("Same content").Value;

        // Assert
        content1.Should().Be(content2);
    }
}
