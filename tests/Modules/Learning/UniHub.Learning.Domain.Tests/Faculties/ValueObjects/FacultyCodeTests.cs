using UniHub.Learning.Domain.Faculties.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Faculties.ValueObjects;

public class FacultyCodeTests
{
    [Fact]
    public void Create_WithValidCode_ShouldReturnSuccess()
    {
        // Arrange
        var code = "CNTT";

        // Act
        var result = FacultyCode.Create(code);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("CNTT"); // Uppercase
    }

    [Theory]
    [InlineData("cntt", "CNTT")]
    [InlineData("toan", "TOAN")]
    [InlineData("  ly  ", "LY")]
    public void Create_ShouldConvertToUppercase(string input, string expected)
    {
        // Act
        var result = FacultyCode.Create(input);

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
        var result = FacultyCode.Create(invalidCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FacultyCode.Empty");
    }

    [Fact]
    public void Create_WithTooShortCode_ShouldReturnFailure()
    {
        // Arrange
        var shortCode = "A";

        // Act
        var result = FacultyCode.Create(shortCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FacultyCode.TooShort");
    }

    [Fact]
    public void Create_WithTooLongCode_ShouldReturnFailure()
    {
        // Arrange
        var longCode = new string('A', FacultyCode.MaxLength + 1);

        // Act
        var result = FacultyCode.Create(longCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FacultyCode.TooLong");
    }

    [Theory]
    [InlineData("CN TT")]
    [InlineData("CN-TT")]
    [InlineData("CN.TT")]
    [InlineData("CN@TT")]
    [InlineData("CN#TT")]
    [InlineData("CNTT!")]
    public void Create_WithInvalidCharacters_ShouldReturnFailure(string invalidCode)
    {
        // Act
        var result = FacultyCode.Create(invalidCode);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("FacultyCode.InvalidFormat");
    }

    [Theory]
    [InlineData("CNTT")]
    [InlineData("TOAN")]
    [InlineData("LY")]
    [InlineData("HOA_HUU_CO")]
    [InlineData("CNTT2023")]
    public void Create_WithValidFormats_ShouldReturnSuccess(string validCode)
    {
        // Act
        var result = FacultyCode.Create(validCode);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var code = "CNTT";
        var facultyCode = FacultyCode.Create(code).Value;

        // Act
        var result = facultyCode.ToString();

        // Assert
        result.Should().Be("CNTT");
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var facultyCode = FacultyCode.Create("CNTT").Value;

        // Act
        string result = facultyCode;

        // Assert
        result.Should().Be("CNTT");
    }
}
