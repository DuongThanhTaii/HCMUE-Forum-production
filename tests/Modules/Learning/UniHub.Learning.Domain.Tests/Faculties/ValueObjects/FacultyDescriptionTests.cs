using UniHub.Learning.Domain.Faculties.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Faculties.ValueObjects;

public class FacultyDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ShouldReturnSuccess()
    {
        // Arrange
        var description = "Khoa đào tạo các ngành liên quan đến Công nghệ Thông tin";

        // Act
        var result = FacultyDescription.Create(description);

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
        var result = FacultyDescription.Create(emptyDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(string.Empty);
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldReturnFailure()
    {
        // Arrange
        var longDescription = new string('a', FacultyDescription.MaxLength + 1);

        // Act
        var result = FacultyDescription.Create(longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FacultyDescription.TooLong");
    }

    [Fact]
    public void Create_WithMaxLengthDescription_ShouldReturnSuccess()
    {
        // Arrange
        var maxDescription = new string('a', FacultyDescription.MaxLength);

        // Act
        var result = FacultyDescription.Create(maxDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var description = "  Valid faculty description  ";

        // Act
        var result = FacultyDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid faculty description");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var description = "Test description";
        var facultyDescription = FacultyDescription.Create(description).Value;

        // Act
        var result = facultyDescription.ToString();

        // Assert
        result.Should().Be(description);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var facultyDescription = FacultyDescription.Create("Test description").Value;

        // Act
        string result = facultyDescription;

        // Assert
        result.Should().Be("Test description");
    }
}
