using UniHub.Learning.Domain.Faculties.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Faculties.ValueObjects;

public class FacultyNameTests
{
    [Fact]
    public void Create_WithValidName_ShouldReturnSuccess()
    {
        // Arrange
        var name = "Khoa Công nghệ Thông tin";

        // Act
        var result = FacultyName.Create(name);

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
        var result = FacultyName.Create(invalidName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FacultyName.Empty");
    }

    [Fact]
    public void Create_WithTooShortName_ShouldReturnFailure()
    {
        // Arrange
        var shortName = "AB";

        // Act
        var result = FacultyName.Create(shortName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FacultyName.TooShort");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('a', FacultyName.MaxLength + 1);

        // Act
        var result = FacultyName.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FacultyName.TooLong");
    }

    [Fact]
    public void Create_WithMaxLengthName_ShouldReturnSuccess()
    {
        // Arrange
        var maxName = new string('a', FacultyName.MaxLength);

        // Act
        var result = FacultyName.Create(maxName);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var name = "  Khoa Công nghệ  ";

        // Act
        var result = FacultyName.Create(name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Khoa Công nghệ");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var name = "Khoa CNTT";
        var facultyName = FacultyName.Create(name).Value;

        // Act
        var result = facultyName.ToString();

        // Assert
        result.Should().Be(name);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var facultyName = FacultyName.Create("Khoa CNTT").Value;

        // Act
        string result = facultyName;

        // Assert
        result.Should().Be("Khoa CNTT");
    }
}
