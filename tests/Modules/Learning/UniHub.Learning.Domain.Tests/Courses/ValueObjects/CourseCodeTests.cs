using UniHub.Learning.Domain.Courses.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Courses.ValueObjects;

public class CourseCodeTests
{
    [Fact]
    public void Create_WithValidCode_ShouldReturnSuccess()
    {
        // Arrange
        var code = "CS101";

        // Act
        var result = CourseCode.Create(code);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("CS101"); // Uppercase
    }

    [Theory]
    [InlineData("cs101", "CS101")]
    [InlineData("math201", "MATH201")]
    [InlineData("  phy301  ", "PHY301")]
    public void Create_ShouldConvertToUppercase(string input, string expected)
    {
        // Act
        var result = CourseCode.Create(input);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnFailure(string? invalidCode)
    {
        // Act
        var result = CourseCode.Create(invalidCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CourseCode.Empty");
    }

    [Fact]
    public void Create_WithTooShortCode_ShouldReturnFailure()
    {
        // Arrange
        var shortCode = "AB";

        // Act
        var result = CourseCode.Create(shortCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CourseCode.TooShort");
    }

    [Fact]
    public void Create_WithTooLongCode_ShouldReturnFailure()
    {
        // Arrange
        var longCode = new string('A', CourseCode.MaxLength + 1);

        // Act
        var result = CourseCode.Create(longCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CourseCode.TooLong");
    }

    [Theory]
    [InlineData("CS 101")]
    [InlineData("CS_101")]
    [InlineData("CS.101")]
    [InlineData("CS@101")]
    [InlineData("CS#101")]
    public void Create_WithInvalidCharacters_ShouldReturnFailure(string invalidCode)
    {
        // Act
        var result = CourseCode.Create(invalidCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CourseCode.InvalidFormat");
    }

    [Theory]
    [InlineData("CS101")]
    [InlineData("MATH-201")]
    [InlineData("PHY301A")]
    [InlineData("CS-123-A")]
    public void Create_WithValidFormats_ShouldReturnSuccess(string validCode)
    {
        // Act
        var result = CourseCode.Create(validCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var code = "CS101";
        var courseCode = CourseCode.Create(code).Value;

        // Act
        var result = courseCode.ToString();

        // Assert
        result.Should().Be("CS101");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var courseCode = CourseCode.Create("CS101").Value;

        // Act
        string result = courseCode;

        // Assert
        result.Should().Be("CS101");
    }
}
