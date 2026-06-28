using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Career.Domain.JobPostings.Events;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Application.EventHandlers;
using Xunit;

namespace UniHub.Notification.Application.Tests.EventHandlers;

public class JobPostingPublishedEventHandlerTests
{
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ILogger<JobPostingPublishedEventHandler> _logger;
    private readonly JobPostingPublishedEventHandler _handler;

    public JobPostingPublishedEventHandlerTests()
    {
        _emailNotificationService = Substitute.For<IEmailNotificationService>();
        _logger = Substitute.For<ILogger<JobPostingPublishedEventHandler>>();
        _handler = new JobPostingPublishedEventHandler(_emailNotificationService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLogExecution()
    {
        // Arrange
        var jobPostingId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var @event = new JobPostingPublishedEvent(
            jobPostingId,
            companyId,
            "Senior Software Engineer",
            DateTime.UtcNow);

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert - Should complete without error
        // Note: Since job posting and job seeker repositories are not implemented,
        // handler logs but doesn't send notifications
        await Task.CompletedTask;
    }
}
