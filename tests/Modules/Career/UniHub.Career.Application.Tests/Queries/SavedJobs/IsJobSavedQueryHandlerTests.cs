using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.SavedJobs.IsSaved;
using Xunit;

namespace UniHub.Career.Application.Tests.Queries.SavedJobs;

public class IsJobSavedQueryHandlerTests
{
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly IsJobSavedQueryHandler _handler;

    public IsJobSavedQueryHandlerTests()
    {
        _savedJobRepository = Substitute.For<ISavedJobRepository>();
        _handler = new IsJobSavedQueryHandler(_savedJobRepository);
    }

    [Fact]
    public async Task Handle_WhenJobIsSaved_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobPostingId = Guid.NewGuid();

        _savedJobRepository.IsSavedAsync(userId, jobPostingId, Arg.Any<CancellationToken>())
            .Returns(true);

        var query = new IsJobSavedQuery(jobPostingId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenJobIsNotSaved_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobPostingId = Guid.NewGuid();

        _savedJobRepository.IsSavedAsync(userId, jobPostingId, Arg.Any<CancellationToken>())
            .Returns(false);

        var query = new IsJobSavedQuery(jobPostingId, userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }
}
