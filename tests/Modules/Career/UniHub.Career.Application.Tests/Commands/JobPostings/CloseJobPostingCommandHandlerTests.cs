using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.JobPostings.CloseJobPosting;
using UniHub.Career.Domain.JobPostings;
using Xunit;

namespace UniHub.Career.Application.Tests.Commands.JobPostings;

public class CloseJobPostingCommandHandlerTests
{
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly CloseJobPostingCommandHandler _handler;

    public CloseJobPostingCommandHandlerTests()
    {
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _handler = new CloseJobPostingCommandHandler(_jobPostingRepository);
    }

    [Fact]
    public async Task Handle_WithValidJobPosting_ShouldClose()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "UX Designer",
            "Design user experiences.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            WorkLocation.Create("Ho Chi Minh City").Value,
            null,
            null).Value;

        jobPosting.Publish();

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var command = new CloseJobPostingCommand(Guid.NewGuid(), "Position filled");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Closed");

        await _jobPostingRepository.Received(1).UpdateAsync(
            Arg.Any<JobPosting>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentJobPosting_ShouldReturnFailure()
    {
        // Arrange
        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns((JobPosting?)null);

        var command = new CloseJobPostingCommand(Guid.NewGuid(), "No longer needed");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JobPosting.NotFound");

        await _jobPostingRepository.DidNotReceive().UpdateAsync(
            Arg.Any<JobPosting>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyClosedJobPosting_ShouldReturnFailure()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "Marketing Manager",
            "Lead marketing efforts.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Senior,
            WorkLocation.Create("Da Nang").Value,
            null,
            null).Value;

        jobPosting.Publish();
        jobPosting.Close("Already filled");

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var command = new CloseJobPostingCommand(Guid.NewGuid(), "Duplicate close");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        await _jobPostingRepository.DidNotReceive().UpdateAsync(
            Arg.Any<JobPosting>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDraftJobPosting_ShouldClose()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "Sales Representative",
            "Sell products.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Entry,
            WorkLocation.Create("Can Tho").Value,
            null,
            null).Value;

        // Draft status

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var command = new CloseJobPostingCommand(Guid.NewGuid(), "Cancelled");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Closed");
    }
}
