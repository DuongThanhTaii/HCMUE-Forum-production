using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.JobPostings.PublishJobPosting;
using UniHub.Career.Domain.JobPostings;
using Xunit;

namespace UniHub.Career.Application.Tests.Commands.JobPostings;

public class PublishJobPostingCommandHandlerTests
{
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly PublishJobPostingCommandHandler _handler;

    public PublishJobPostingCommandHandlerTests()
    {
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _handler = new PublishJobPostingCommandHandler(_jobPostingRepository);
    }

    [Fact]
    public async Task Handle_WithValidDraftJobPosting_ShouldPublish()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "Backend Developer",
            "Build scalable APIs.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            WorkLocation.Create("Ho Chi Minh City").Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var command = new PublishJobPostingCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Published");
        result.Value.PublishedAt.Should().NotBeNull();

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

        var command = new PublishJobPostingCommand(Guid.NewGuid());

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
    public async Task Handle_WithAlreadyPublishedJobPosting_ShouldReturnFailure()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "Data Analyst",
            "Analyze data.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Junior,
            WorkLocation.Create("Da Nang").Value,
            null,
            null).Value;

        jobPosting.Publish(); // Already published

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var command = new PublishJobPostingCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        await _jobPostingRepository.DidNotReceive().UpdateAsync(
            Arg.Any<JobPosting>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithClosedJobPosting_ShouldReturnFailure()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "Product Manager",
            "Manage products.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Senior,
            WorkLocation.Create("Ha Noi").Value,
            null,
            null).Value;

        jobPosting.Publish();
        jobPosting.Close("Position filled");

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var command = new PublishJobPostingCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }
}
