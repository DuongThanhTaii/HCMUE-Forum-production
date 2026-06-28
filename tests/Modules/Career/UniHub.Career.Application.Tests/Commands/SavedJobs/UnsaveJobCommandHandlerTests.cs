using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.SavedJobs.UnsaveJob;
using Xunit;

namespace UniHub.Career.Application.Tests.Commands.SavedJobs;

public class UnsaveJobCommandHandlerTests
{
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly UnsaveJobCommandHandler _handler;

    public UnsaveJobCommandHandlerTests()
    {
        _savedJobRepository = Substitute.For<ISavedJobRepository>();
        _handler = new UnsaveJobCommandHandler(_savedJobRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUnsaveJob()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobPostingId = Guid.NewGuid();

        _savedJobRepository.IsSavedAsync(userId, jobPostingId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UnsaveJobCommand(jobPostingId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _savedJobRepository.Received(1).UnsaveJobAsync(userId, jobPostingId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonSavedJob_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobPostingId = Guid.NewGuid();

        _savedJobRepository.IsSavedAsync(userId, jobPostingId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UnsaveJobCommand(jobPostingId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SavedJob.NotFound");
        await _savedJobRepository.DidNotReceive().UnsaveJobAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
