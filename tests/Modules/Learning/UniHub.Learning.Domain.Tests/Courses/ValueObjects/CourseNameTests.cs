using UniHub.Learning.Domain.Courses.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Courses.ValueObjects;

public class CourseNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldReturnSuccess()
    {
        // Arrange
        var name = "Introduction to Computer Science";

        // Act
        var result = CourseName.Create(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnFailure(string? invalidName)
    {
        // Act
        var result = CourseName.Create(invalidName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CourseName.Empty");
    }

    [Fact]
    public void Create_WithTooShortName_ShouldReturnFailure()
    {
        // Arrange
        var shortName = "AB";

        // Act
        var result = CourseName.Create(shortName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CourseName.TooShort");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('a', CourseName.MaxLength + 1);

        // Act
        var result = CourseName.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CourseName.TooLong");
    }

    [Fact]
    public void Create_WithMaxLengthName_ShouldReturnSuccess()
    {
        // Arrange
        var maxName = new string('a', CourseName.MaxLength);

        // Act
        var result = CourseName.Create(maxName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var name = "  Valid Course Name  ";

        // Act
        var result = CourseName.Create(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid Course Name");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var name = "Test Course";
        var courseName = CourseName.Create(name).Value;

        // Act
        var result = courseName.ToString();

        // Assert
        result.Should().Be(name);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var courseName = CourseName.Create("Test Course").Value;

        // Act
        string result = courseName;

        // Assert
        result.Should().Be("Test Course");
    }
}
