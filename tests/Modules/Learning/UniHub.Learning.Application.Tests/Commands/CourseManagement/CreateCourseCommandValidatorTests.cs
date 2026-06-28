using FluentAssertions;
using UniHub.Learning.Application.Commands.CourseManagement;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.CourseManagement;

public class CreateCourseCommandValidatorTests
{
    private readonly CreateCourseCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.NewGuid(),
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCode_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code");
    }

    [Fact]
    public void Validate_WithInvalidCodeFormat_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "cs-101",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage.Contains("uppercase"));
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "",
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithNameTooShort_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "AB",
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            new string('a', 201),
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            new string('a', 2001),
            "2024-1",
            3,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public void Validate_WithEmptySemester_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "",
            3,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Semester");
    }

    [Fact]
    public void Validate_WithCreditsLessThanOne_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            0,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Credits");
    }

    [Fact]
    public void Validate_WithCreditsGreaterThanTen_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            11,
            Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Credits");
    }

    [Fact]
    public void Validate_WithEmptyCreatedBy_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CreatedBy");
    }

    [Fact]
    public void Validate_WithEmptyFacultyId_ShouldFail()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.NewGuid(),
            Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FacultyId");
    }

    [Fact]
    public void Validate_WithNullFacultyId_ShouldPass()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            3,
            Guid.NewGuid(),
            null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
