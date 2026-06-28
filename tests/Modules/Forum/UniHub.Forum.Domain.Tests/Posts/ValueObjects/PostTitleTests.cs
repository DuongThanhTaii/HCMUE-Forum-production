using UniHub.Forum.Domain.Posts.ValueObjects;

namespace UniHub.Forum.Domain.Tests.Posts.ValueObjects;

public class PostTitleTests
{
    [Fact]
    public void Create_WithValidTitle_ShouldReturnSuccess()
    {
        // Arrange
        var titleValue = "This is a valid post title";

        // Act
        var result = PostTitle.Create(titleValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(titleValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceTitle_ShouldReturnFailure(string? titleValue)
    {
        // Act
        var result = PostTitle.Create(titleValue!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PostTitle.Empty");
    }

    [Fact]
    public void Create_WithTitleTooShort_ShouldReturnFailure()
    {
        // Arrange
        var titleValue = "Hi";

        // Act
        var result = PostTitle.Create(titleValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PostTitle.TooShort");
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldReturnFailure()
    {
        // Arrange
        var titleValue = new string('a', PostTitle.MaxLength + 1);

        // Act
        var result = PostTitle.Create(titleValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PostTitle.TooLong");
    }

    [Fact]
    public void Create_WithTitleAtMinLength_ShouldReturnSuccess()
    {
        // Arrange
        var titleValue = new string('a', PostTitle.MinLength);

        // Act
        var result = PostTitle.Create(titleValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithTitleAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var titleValue = new string('a', PostTitle.MaxLength);

        // Act
        var result = PostTitle.Create(titleValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        var titleValue = "  Valid Title  ";

        // Act
        var result = PostTitle.Create(titleValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid Title");
    }

    [Fact]
    public void TwoPostTitlesWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var title1 = PostTitle.Create("Same Title").Value;
        var title2 = PostTitle.Create("Same Title").Value;

        // Assert
        title1.Should().Be(title2);
    }
}
