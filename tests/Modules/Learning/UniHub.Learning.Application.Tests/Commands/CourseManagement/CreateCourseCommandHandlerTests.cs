using FluentAssertions;
using NSubstitute;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Application.Commands.CourseManagement;
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Courses.ValueObjects;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.CourseManagement;

public class CreateCourseCommandHandlerTests
{
    private readonly ICourseRepository _courseRepository;
    private readonly CreateCourseCommandHandler _handler;

    public CreateCourseCommandHandlerTests()
    {
        _courseRepository = Substitute.For<ICourseRepository>();
        _handler = new CreateCourseCommandHandler(_courseRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessWithCourseId()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic concepts",
            "2024-1",
            3,
            Guid.NewGuid(),
            Guid.NewGuid());

        _courseRepository.ExistsByCodeAsync(command.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _courseRepository.Received(1).AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingCourseCode_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic concepts",
            "2024-1",
            3,
            Guid.NewGuid());

        _courseRepository.ExistsByCodeAsync(command.Code, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.CodeAlreadyExists");
        await _courseRepository.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCourseCode_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "AB", // Too short
            "Introduction to Computer Science",
            "Basic concepts",
            "2024-1",
            3,
            Guid.NewGuid());

        _courseRepository.ExistsByCodeAsync(command.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _courseRepository.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCourseName_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "AB", // Too short
            "Basic concepts",
            "2024-1",
            3,
            Guid.NewGuid());

        _courseRepository.ExistsByCodeAsync(command.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        await _courseRepository.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCredits_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateCourseCommand(
            "CS101",
            "Introduction to Computer Science",
            "Basic concepts",
            "2024-1",
            0, // Invalid credits
            Guid.NewGuid());

        _courseRepository.ExistsByCodeAsync(command.Code, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidCredits");
        await _courseRepository.DidNotReceive().AddAsync(Arg.Any<Course>(), Arg.Any<CancellationToken>());
    }
}
