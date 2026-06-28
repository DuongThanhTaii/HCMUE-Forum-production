using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.JobPostings.UpdateJobPosting;
using UniHub.Career.Domain.JobPostings;
using Xunit;

namespace UniHub.Career.Application.Tests.Commands.JobPostings;

public class UpdateJobPostingCommandHandlerTests
{
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly UpdateJobPostingCommandHandler _handler;

    public UpdateJobPostingCommandHandlerTests()
    {
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _handler = new UpdateJobPostingCommandHandler(_jobPostingRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateJobPosting()
    {
        // Arrange
        var jobPostingId = Guid.NewGuid();
        var existingJobPosting = JobPosting.Create(
            ".NET Developer",
            "Old description.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Junior,
            WorkLocation.Create("Ha Noi").Value,
            null,
            null).Value;

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(existingJobPosting);

        var command = new UpdateJobPostingCommand(
            JobPostingId: jobPostingId,
            Title: "Senior .NET Developer",
            Description: "New description with updated requirements.",
            JobType: JobType.Remote,
            ExperienceLevel: ExperienceLevel.Senior,
            City: "Ho Chi Minh City",
            District: "District 7",
            IsRemote: true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Senior .NET Developer");
        result.Value.ExperienceLevel.Should().Be("Senior");
        result.Value.Location.City.Should().Be("Ho Chi Minh City");
        result.Value.Location.IsRemote.Should().BeTrue();

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

        var command = new UpdateJobPostingCommand(
            JobPostingId: Guid.NewGuid(),
            Title: "Updated Title",
            Description: "Updated description.",
            JobType: JobType.FullTime,
            ExperienceLevel: ExperienceLevel.Mid,
            City: "Da Nang");

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
    public async Task Handle_WithPublishedJobPosting_ShouldReturnFailure()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "DevOps Engineer",
            "Manage cloud infrastructure.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Senior,
            WorkLocation.Create("Singapore").Value,
            null,
            null).Value;

        jobPosting.Publish(); // Make it published

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var command = new UpdateJobPostingCommand(
            JobPostingId: Guid.NewGuid(),
            Title: "Updated Title",
            Description: "Updated description.",
            JobType: JobType.FullTime,
            ExperienceLevel: ExperienceLevel.Lead,
            City: "Singapore");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        await _jobPostingRepository.DidNotReceive().UpdateAsync(
            Arg.Any<JobPosting>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSalaryUpdate_ShouldUpdateCorrectly()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "Frontend Developer",
            "Build UIs.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Junior,
            WorkLocation.Create("Ha Noi").Value,
            null,
            null).Value;

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var command = new UpdateJobPostingCommand(
            JobPostingId: Guid.NewGuid(),
            Title: "Frontend Developer",
            Description: "Build UIs.",
            JobType: JobType.FullTime,
            ExperienceLevel: ExperienceLevel.Junior,
            City: "Ha Noi",
            MinSalary: 1500m,
            MaxSalary: 2500m,
            SalaryCurrency: "USD",
            SalaryPeriod: "month");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Salary.Should().NotBeNull();
        result.Value.Salary!.MinAmount.Should().Be(1500m);
        result.Value.Salary.MaxAmount.Should().Be(2500m);
    }
}
