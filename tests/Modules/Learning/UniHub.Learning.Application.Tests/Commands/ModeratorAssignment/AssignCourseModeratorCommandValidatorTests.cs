using FluentAssertions;
using UniHub.Learning.Application.Commands.ModeratorAssignment;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.ModeratorAssignment;

public class AssignCourseModeratorCommandValidatorTests
{
    private readonly AssignCourseModeratorCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new AssignCourseModeratorCommand(
            CourseId: Guid.NewGuid(),
            ModeratorId: Guid.NewGuid(),
            AssignedBy: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCourseId_ShouldFail()
    {
        // Arrange
        var command = new AssignCourseModeratorCommand(
            CourseId: Guid.Empty,
            ModeratorId: Guid.NewGuid(),
            AssignedBy: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Course ID is required");
    }

    [Fact]
    public void Validate_WithEmptyModeratorId_ShouldFail()
    {
        // Arrange
        var command = new AssignCourseModeratorCommand(
            CourseId: Guid.NewGuid(),
            ModeratorId: Guid.Empty,
            AssignedBy: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Moderator ID is required");
    }

    [Fact]
    public void Validate_WithEmptyAssignedBy_ShouldFail()
    {
        // Arrange
        var command = new AssignCourseModeratorCommand(
            CourseId: Guid.NewGuid(),
            ModeratorId: Guid.NewGuid(),
            AssignedBy: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Assigned By ID is required");
    }
}
