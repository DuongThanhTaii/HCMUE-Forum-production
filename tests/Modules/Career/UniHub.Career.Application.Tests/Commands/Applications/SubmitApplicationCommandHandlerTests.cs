using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.Applications.SubmitApplication;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using Xunit;
using DomainApplication = UniHub.Career.Domain.Applications.Application;
using DomainApplicationId = UniHub.Career.Domain.Applications.ApplicationId;

namespace UniHub.Career.Application.Tests.Commands.Applications;

public class SubmitApplicationCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly SubmitApplicationCommandHandler _handler;

    public SubmitApplicationCommandHandlerTests()
    {
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _handler = new SubmitApplicationCommandHandler(_applicationRepository, _jobPostingRepository);
    }

    [Fact]
    public async Task Handle_WithNonExistentJobPosting_ShouldReturnFailure()
    {
        // Arrange
        _jobPostingRepository.GetByIdAsync(Arg.Any<JobPostingId>(), Arg.Any<CancellationToken>())
            .Returns((JobPosting?)null);

        var command = new SubmitApplicationCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "resume.pdf",
            "https://example.com/resume.pdf",
            1024000,
            "application/pdf");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JobPosting.NotFound");
        await _applicationRepository.DidNotReceive().AddAsync(Arg.Any<DomainApplication>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateApplication_ShouldReturnFailure()
    {
        // Arrange
        var jobPostingId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();

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

        jobPosting.Publish();

        var existingApplication = DomainApplication.Submit(
            JobPostingId.Create(jobPostingId),
            applicantId,
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        _jobPostingRepository.GetByIdAsync(Arg.Any<JobPostingId>(), Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        _applicationRepository.GetByJobAndApplicantAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<Guid>(),
            Arg.Any<CancellationToken>())
            .Returns(existingApplication);

        var command = new SubmitApplicationCommand(
            jobPostingId,
            applicantId,
            "resume2.pdf",
            "https://example.com/resume2.pdf",
            1024000,
            "application/pdf");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.AlreadyExists");
        await _applicationRepository.DidNotReceive().AddAsync(Arg.Any<DomainApplication>(), Arg.Any<CancellationToken>());
    }
}
