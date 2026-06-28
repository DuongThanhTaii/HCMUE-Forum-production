using UniHub.Forum.Domain.Categories.ValueObjects;

namespace UniHub.Forum.Domain.Tests.Categories.ValueObjects;

public class CategoryDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ShouldReturnSuccess()
    {
        // Arrange
        var descriptionValue = "A category for technology discussions";

        // Act
        var result = CategoryDescription.Create(descriptionValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(descriptionValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceDescription_ShouldReturnEmptyString(string? descriptionValue)
    {
        // Act
        var result = CategoryDescription.Create(descriptionValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(string.Empty);
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldReturnFailure()
    {
        // Arrange
        var descriptionValue = new string('a', CategoryDescription.MaxLength + 1);

        // Act
        var result = CategoryDescription.Create(descriptionValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CategoryDescription.TooLong");
    }

    [Fact]
    public void Create_WithDescriptionAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var descriptionValue = new string('a', CategoryDescription.MaxLength);

        // Act
        var result = CategoryDescription.Create(descriptionValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        var descriptionValue = "  Tech category  ";

        // Act
        var result = CategoryDescription.Create(descriptionValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Tech category");
    }

    [Fact]
    public void TwoCategoryDescriptionsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var desc1 = CategoryDescription.Create("Same description").Value;
        var desc2 = CategoryDescription.Create("Same description").Value;

        // Assert
        desc1.Should().Be(desc2);
    }
}
