using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.Applications.WithdrawApplication;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using Xunit;
using DomainApplication = UniHub.Career.Domain.Applications.Application;
using DomainApplicationId = UniHub.Career.Domain.Applications.ApplicationId;

namespace UniHub.Career.Application.Tests.Commands.Applications;

public class WithdrawApplicationCommandHandlerTests
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly WithdrawApplicationCommandHandler _handler;

    public WithdrawApplicationCommandHandlerTests()
    {
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _handler = new WithdrawApplicationCommandHandler(_applicationRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldWithdrawApplication()
    {
        // Arrange
        var jobPostingId = Guid.NewGuid();
        var applicantId = Guid.NewGuid();

        var application = DomainApplication.Submit(
            JobPostingId.Create(jobPostingId),
            applicantId,
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new WithdrawApplicationCommand(
            application.Id.Value,
            applicantId,
            "Found better opportunity");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Withdrawn);
        await _applicationRepository.Received(1).UpdateAsync(application, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentApplication_ShouldReturnFailure()
    {
        // Arrange
        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns((DomainApplication?)null);

        var command = new WithdrawApplicationCommand(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.NotFound");
        await _applicationRepository.DidNotReceive().UpdateAsync(Arg.Any<DomainApplication>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWrongApplicantId_ShouldReturnFailure()
    {
        // Arrange
        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(), // Original applicant
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new WithdrawApplicationCommand(
            application.Id.Value,
            Guid.NewGuid()); // Different applicant trying to withdraw

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.NotApplicant");
        await _applicationRepository.DidNotReceive().UpdateAsync(Arg.Any<DomainApplication>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyWithdrawnApplication_ShouldReturnFailure()
    {
        // Arrange
        var applicantId = Guid.NewGuid();

        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            applicantId,
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        application.Withdraw(applicantId); // Already withdrawn

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new WithdrawApplicationCommand(application.Id.Value, applicantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.AlreadyWithdrawn");
    }

    [Fact]
    public async Task Handle_WithAcceptedApplication_ShouldReturnFailure()
    {
        // Arrange
        var applicantId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var application = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            applicantId,
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        application.Offer(reviewerId);
        application.Accept(applicantId); // Application accepted

        _applicationRepository.GetByIdAsync(Arg.Any<DomainApplicationId>(), Arg.Any<CancellationToken>())
            .Returns(application);

        var command = new WithdrawApplicationCommand(application.Id.Value, applicantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.CannotWithdrawAfterAccepted");
    }
}
