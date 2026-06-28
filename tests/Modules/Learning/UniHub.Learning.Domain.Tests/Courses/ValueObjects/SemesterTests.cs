using UniHub.Learning.Domain.Courses.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Courses.ValueObjects;

public class SemesterTests
{
    [Fact]
    public void Create_WithValidSemester_ShouldReturnSuccess()
    {
        // Arrange
        var semester = "2024-2025 HK1";

        // Act
        var result = Semester.Create(semester);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(semester);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnFailure(string? invalidSemester)
    {
        // Act
        var result = Semester.Create(invalidSemester);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Semester.Empty");
    }

    [Fact]
    public void Create_WithTooShortSemester_ShouldReturnFailure()
    {
        // Arrange
        var shortSemester = "ABC";

        // Act
        var result = Semester.Create(shortSemester);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Semester.TooShort");
    }

    [Fact]
    public void Create_WithTooLongSemester_ShouldReturnFailure()
    {
        // Arrange
        var longSemester = new string('a', Semester.MaxLength + 1);

        // Act
        var result = Semester.Create(longSemester);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Semester.TooLong");
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var semester = "  2024-2025 HK1  ";

        // Act
        var result = Semester.Create(semester);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("2024-2025 HK1");
    }

    [Theory]
    [InlineData(2024, 1, "2024-2025 HK1")]
    [InlineData(2023, 2, "2023-2024 HK2")]
    [InlineData(2025, 3, "2025-2026 HK3")]
    public void CreateFromYearAndTerm_WithValidInput_ShouldReturnSuccess(int year, int term, string expected)
    {
        // Act
        var result = Semester.CreateFromYearAndTerm(year, term);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
    }

    [Theory]
    [InlineData(1999)]
    [InlineData(2101)]
    public void CreateFromYearAndTerm_WithInvalidYear_ShouldReturnFailure(int invalidYear)
    {
        // Act
        var result = Semester.CreateFromYearAndTerm(invalidYear, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Semester.InvalidYear");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(-1)]
    public void CreateFromYearAndTerm_WithInvalidTerm_ShouldReturnFailure(int invalidTerm)
    {
        // Act
        var result = Semester.CreateFromYearAndTerm(2024, invalidTerm);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Semester.InvalidTerm");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var semester = "2024-2025 HK1";
        var semesterObj = Semester.Create(semester).Value;

        // Act
        var result = semesterObj.ToString();

        // Assert
        result.Should().Be(semester);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var semesterObj = Semester.Create("2024-2025 HK1").Value;

        // Act
        string result = semesterObj;

        // Assert
        result.Should().Be("2024-2025 HK1");
    }
}
