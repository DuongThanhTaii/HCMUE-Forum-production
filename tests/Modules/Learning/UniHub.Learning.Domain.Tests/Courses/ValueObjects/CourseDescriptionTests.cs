using UniHub.Learning.Domain.Courses.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Courses.ValueObjects;

public class CourseDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ShouldReturnSuccess()
    {
        // Arrange
        var description = "This course covers fundamental concepts of computer science.";

        // Act
        var result = CourseDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_WithNullOrEmpty_ShouldReturnSuccessWithEmptyString(string? emptyDescription)
    {
        // Act
        var result = CourseDescription.Create(emptyDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(string.Empty);
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldReturnFailure()
    {
        // Arrange
        var longDescription = new string('a', CourseDescription.MaxLength + 1);

        // Act
        var result = CourseDescription.Create(longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CourseDescription.TooLong");
    }

    [Fact]
    public void Create_WithMaxLengthDescription_ShouldReturnSuccess()
    {
        // Arrange
        var maxDescription = new string('a', CourseDescription.MaxLength);

        // Act
        var result = CourseDescription.Create(maxDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var description = "  Valid course description  ";

        // Act
        var result = CourseDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid course description");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var description = "Test description";
        var courseDescription = CourseDescription.Create(description).Value;

        // Act
        var result = courseDescription.ToString();

        // Assert
        result.Should().Be(description);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var courseDescription = CourseDescription.Create("Test description").Value;

        // Act
        string result = courseDescription;

        // Assert
        result.Should().Be("Test description");
    }
}
