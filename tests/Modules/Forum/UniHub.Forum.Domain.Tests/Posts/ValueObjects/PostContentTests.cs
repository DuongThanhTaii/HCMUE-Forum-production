using UniHub.Forum.Domain.Posts.ValueObjects;

namespace UniHub.Forum.Domain.Tests.Posts.ValueObjects;

public class PostContentTests
{
    [Fact]
    public void Create_WithValidContent_ShouldReturnSuccess()
    {
        // Arrange
        var contentValue = "This is a valid post content with enough characters.";

        // Act
        var result = PostContent.Create(contentValue);

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
        var result = PostContent.Create(contentValue!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PostContent.Empty");
    }

    [Fact]
    public void Create_WithContentTooShort_ShouldReturnFailure()
    {
        // Arrange
        var contentValue = "Short";

        // Act
        var result = PostContent.Create(contentValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PostContent.TooShort");
    }

    [Fact]
    public void Create_WithContentTooLong_ShouldReturnFailure()
    {
        // Arrange
        var contentValue = new string('a', PostContent.MaxLength + 1);

        // Act
        var result = PostContent.Create(contentValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PostContent.TooLong");
    }

    [Fact]
    public void Create_WithContentAtMinLength_ShouldReturnSuccess()
    {
        // Arrange
        var contentValue = new string('a', PostContent.MinLength);

        // Act
        var result = PostContent.Create(contentValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithContentAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var contentValue = new string('a', PostContent.MaxLength);

        // Act
        var result = PostContent.Create(contentValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        var contentValue = "  Valid content with enough characters  ";

        // Act
        var result = PostContent.Create(contentValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid content with enough characters");
    }
}
