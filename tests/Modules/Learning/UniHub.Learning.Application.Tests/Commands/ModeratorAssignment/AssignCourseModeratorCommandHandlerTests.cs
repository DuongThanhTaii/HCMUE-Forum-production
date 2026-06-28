using FluentAssertions;
using NSubstitute;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Application.Commands.ModeratorAssignment;
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Courses.ValueObjects;
using UniHub.SharedKernel.Results;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.ModeratorAssignment;

public class AssignCourseModeratorCommandHandlerTests
{
    private readonly ICourseRepository _courseRepository = Substitute.For<ICourseRepository>();
    private readonly IModeratorManagementPermissionService _permissionService = Substitute.For<IModeratorManagementPermissionService>();
    private readonly AssignCourseModeratorCommandHandler _handler;

    public AssignCourseModeratorCommandHandlerTests()
    {
        _handler = new AssignCourseModeratorCommandHandler(_courseRepository, _permissionService);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        var command = new AssignCourseModeratorCommand(courseId, moderatorId, assignedBy);

        var course = Course.Create(
            CourseCode.Create("CS101").Value,
            CourseName.Create("Programming").Value,
            CourseDescription.Create("Learn programming").Value,
            Semester.Create("Fall 2026").Value,
            3,
            Guid.NewGuid(),
            null).Value;

        _permissionService.CanManageCourseModeratorsAsync(assignedBy, courseId, Arg.Any<CancellationToken>())
            .Returns(true);

        _courseRepository.GetByIdAsync(Arg.Any<CourseId>(), Arg.Any<CancellationToken>())
            .Returns(course);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _courseRepository.Received(1).UpdateAsync(course, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailure()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        var command = new AssignCourseModeratorCommand(courseId, moderatorId, assignedBy);

        _permissionService.CanManageCourseModeratorsAsync(assignedBy, courseId, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.Unauthorized");
        await _courseRepository.DidNotReceive().GetByIdAsync(Arg.Any<CourseId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentCourse_ShouldReturnFailure()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        var command = new AssignCourseModeratorCommand(courseId, moderatorId, assignedBy);

        _permissionService.CanManageCourseModeratorsAsync(assignedBy, courseId, Arg.Any<CancellationToken>())
            .Returns(true);

        _courseRepository.GetByIdAsync(Arg.Any<CourseId>(), Arg.Any<CancellationToken>())
            .Returns((Course?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.NotFound");
        await _courseRepository.DidNotReceive().UpdateAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyAssignedModerator_ShouldReturnFailure()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var moderatorId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        var command = new AssignCourseModeratorCommand(courseId, moderatorId, assignedBy);

        var course = Course.Create(
            CourseCode.Create("CS101").Value,
            CourseName.Create("Programming").Value,
            CourseDescription.Create("Learn programming").Value,
            Semester.Create("Fall 2026").Value,
            3,
            Guid.NewGuid(),
            null).Value;

        // Assign moderator first time
        course.AssignModerator(moderatorId, Guid.NewGuid());

        _permissionService.CanManageCourseModeratorsAsync(assignedBy, courseId, Arg.Any<CancellationToken>())
            .Returns(true);

        _courseRepository.GetByIdAsync(Arg.Any<CourseId>(), Arg.Any<CancellationToken>())
            .Returns(course);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.ModeratorAlreadyAssigned");
        await _courseRepository.DidNotReceive().UpdateAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }
}
