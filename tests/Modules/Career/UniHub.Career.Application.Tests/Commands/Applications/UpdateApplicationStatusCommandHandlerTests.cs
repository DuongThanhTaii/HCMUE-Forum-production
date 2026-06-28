using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.Applications.UpdateApplicationStatus;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using Xunit;
using DomainApplication = UniHub.Career.Domain.Applications.Application;
using DomainApplicationId = UniHub.Career.Domain.Applications.ApplicationId;

namespace UniHub.Career.Application.Tests.Commands.Applications;

public class UpdateApplicationStatusCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly UpdateApplicationStatusCommandHandler _handler;

    public UpdateApplicationStatusCommandHandlerTests()
    {
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _handler = new UpdateApplicationStatusCommandHandler(_applicationRepository);
    }

    [Fact]
    public async Task Handle_MoveToReviewing_ShouldUpdateStatus()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();

        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new UpdateApplicationStatusCommand(
            application.Id.Value,
            reviewerId,
            "Reviewing");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Reviewing);
        await _applicationRepository.Received(1).UpdateAsync(application, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Shortlist_ShouldUpdateStatusWithNotes()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();

        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new UpdateApplicationStatusCommand(
            application.Id.Value,
            reviewerId,
            "Shortlisted",
            "Good technical skills");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Shortlisted);
        application.ReviewNotes.Should().Be("Good technical skills");
    }

    [Fact]
    public async Task Handle_MarkAsInterviewed_ShouldUpdateStatus()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();

        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        application.MoveToReviewing(reviewerId);
        application.Shortlist(reviewerId);

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new UpdateApplicationStatusCommand(
            application.Id.Value,
            reviewerId,
            "Interviewed");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Interviewed);
    }

    [Fact]
    public async Task Handle_Offer_ShouldUpdateStatusToOffered()
    {
        // Arrange
        var reviewerId = Guid.NewGuid();

        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        application.MoveToReviewing(reviewerId);
        application.Shortlist(reviewerId);
        application.MarkAsInterviewed(reviewerId);

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new UpdateApplicationStatusCommand(
            application.Id.Value,
            reviewerId,
            "Offered",
            "Competitive salary package");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Offered);
        application.ReviewNotes.Should().Be("Competitive salary package");
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldReturnFailure()
    {
        // Arrange
        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new UpdateApplicationStatusCommand(
            application.Id.Value,
            Guid.NewGuid(),
            "InvalidStatus");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.InvalidStatus");
        await _applicationRepository.DidNotReceive().UpdateAsync(Arg.Any<DomainApplication>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonReviewableStatus_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateApplicationStatusCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Accepted"); // Cannot use this command for Accept

        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.InvalidStatusTransition");
    }
}
