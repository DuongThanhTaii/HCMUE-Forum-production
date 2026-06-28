using FluentAssertions;
using UniHub.Learning.Application.Commands.CourseManagement;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.CourseManagement;

public class UpdateCourseCommandValidatorTests
{
    private readonly UpdateCourseCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            3);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCourseId_ShouldFail()
    {
        // Arrange
        var command = new UpdateCourseCommand(
            Guid.Empty,
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            3);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CourseId");
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            "",
            "Basic computer science concepts",
            "2024-1",
            3);

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
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            "AB",
            "Basic computer science concepts",
            "2024-1",
            3);

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
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            new string('a', 201),
            "Basic computer science concepts",
            "2024-1",
            3);

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
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            "Introduction to Computer Science",
            new string('a', 2001),
            "2024-1",
            3);

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
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "",
            3);

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
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            0);

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
        var command = new UpdateCourseCommand(
            Guid.NewGuid(),
            "Introduction to Computer Science",
            "Basic computer science concepts",
            "2024-1",
            11);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Credits");
    }
}
