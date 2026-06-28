using UniHub.Forum.Domain.Categories.ValueObjects;

namespace UniHub.Forum.Domain.Tests.Categories.ValueObjects;

public class CategoryNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldReturnSuccess()
    {
        // Arrange
        var nameValue = "Technology";

        // Act
        var result = CategoryName.Create(nameValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(nameValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceName_ShouldReturnFailure(string? nameValue)
    {
        // Act
        var result = CategoryName.Create(nameValue!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CategoryName.Empty");
    }

    [Fact]
    public void Create_WithNameTooShort_ShouldReturnFailure()
    {
        // Arrange
        var nameValue = "A";

        // Act
        var result = CategoryName.Create(nameValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CategoryName.TooShort");
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldReturnFailure()
    {
        // Arrange
        var nameValue = new string('a', CategoryName.MaxLength + 1);

        // Act
        var result = CategoryName.Create(nameValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CategoryName.TooLong");
    }

    [Fact]
    public void Create_WithNameAtMinLength_ShouldReturnSuccess()
    {
        // Arrange
        var nameValue = new string('a', CategoryName.MinLength);

        // Act
        var result = CategoryName.Create(nameValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNameAtMaxLength_ShouldReturnSuccess()
    {
        // Arrange
        var nameValue = new string('a', CategoryName.MaxLength);

        // Act
        var result = CategoryName.Create(nameValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        var nameValue = "  Technology  ";

        // Act
        var result = CategoryName.Create(nameValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Technology");
    }

    [Fact]
    public void TwoCategoryNamesWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var name1 = CategoryName.Create("Technology").Value;
        var name2 = CategoryName.Create("Technology").Value;

        // Assert
        name1.Should().Be(name2);
    }
}
