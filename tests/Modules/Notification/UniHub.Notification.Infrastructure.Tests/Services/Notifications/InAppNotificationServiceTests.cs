using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.Notification.Infrastructure.Services.Notifications;
using Xunit;

namespace UniHub.Notification.Infrastructure.Tests.Services.Notifications;

/// <summary>
/// Unit tests for InAppNotificationService.
/// </summary>
public class InAppNotificationServiceTests
{
    private readonly InAppNotificationService _service;
    private readonly ILogger<InAppNotificationService> _logger;

    public InAppNotificationServiceTests()
    {
        _logger = Substitute.For<ILogger<InAppNotificationService>>();
        _service = new InAppNotificationService(_logger);
    }

    [Fact]
    public void Channel_ShouldReturnInApp()
    {
        _service.Channel.Should().Be(NotificationChannel.InApp);
    }

    [Fact]
    public async Task SendAsync_WithValidNotification_ShouldReturnSuccess()
    {
        // Arrange
        var content = NotificationContent.Create(
            "Test Notification",
            "You have a new message",
            "/messages",
            "https://icon.url").Value;

        var notification = Domain.Notifications.Notification.Create(
            Guid.NewGuid(),
            NotificationChannel.InApp,
            content).Value;

        // Act
        var result = await _service.SendAsync(notification);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task SendAsync_ShouldLogInformation()
    {
        // Arrange
        var content = NotificationContent.Create(
            "Test",
            "Body",
            null,
            null).Value;

        var recipientId = Guid.NewGuid();
        var notification = Domain.Notifications.Notification.Create(
            recipientId,
            NotificationChannel.InApp,
            content).Value;

        // Act
        var result = await _service.SendAsync(notification);

        // Assert - verify result and that logger was called
        result.IsSuccess.Should().BeTrue();
        _logger.ReceivedCalls().Should().NotBeEmpty();
    }
}
