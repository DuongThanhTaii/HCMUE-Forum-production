using FluentAssertions;
using UniHub.Learning.Application.Commands.CourseManagement;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.CourseManagement;

public class DeleteCourseCommandValidatorTests
{
    private readonly DeleteCourseCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new DeleteCourseCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyCourseId_ShouldFail()
    {
        // Arrange
        var command = new DeleteCourseCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CourseId");
    }

    [Fact]
    public void Validate_WithEmptyDeletedBy_ShouldFail()
    {
        // Arrange
        var command = new DeleteCourseCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DeletedBy");
    }
}
