using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.SavedJobs.SaveJob;
using UniHub.Career.Domain.JobPostings;
using Xunit;

namespace UniHub.Career.Application.Tests.Commands.SavedJobs;

public class SaveJobCommandHandlerTests
{
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly SaveJobCommandHandler _handler;

    public SaveJobCommandHandlerTests()
    {
        _savedJobRepository = Substitute.For<ISavedJobRepository>();
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _handler = new SaveJobCommandHandler(_savedJobRepository, _jobPostingRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldSaveJob()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobPostingId = Guid.NewGuid();

        var jobPosting = JobPosting.Create(
            "Software Engineer",
            "Job description",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            WorkLocation.Create("Hanoi", null, null, false).Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;

        _jobPostingRepository.GetByIdAsync(Arg.Any<JobPostingId>(), Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        _savedJobRepository.IsSavedAsync(userId, jobPostingId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new SaveJobCommand(jobPostingId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _savedJobRepository.Received(1).SaveJobAsync(userId, jobPostingId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentJobPosting_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobPostingId = Guid.NewGuid();

        _jobPostingRepository.GetByIdAsync(Arg.Any<JobPostingId>(), Arg.Any<CancellationToken>())
            .Returns((JobPosting?)null);

        var command = new SaveJobCommand(jobPostingId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JobPosting.NotFound");
        await _savedJobRepository.DidNotReceive().SaveJobAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadySavedJob_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobPostingId = Guid.NewGuid();

        var jobPosting = JobPosting.Create(
            "Software Engineer",
            "Job description",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            WorkLocation.Create("Hanoi", null, null, false).Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;

        _jobPostingRepository.GetByIdAsync(Arg.Any<JobPostingId>(), Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        _savedJobRepository.IsSavedAsync(userId, jobPostingId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new SaveJobCommand(jobPostingId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SavedJob.AlreadyExists");
        await _savedJobRepository.DidNotReceive().SaveJobAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
