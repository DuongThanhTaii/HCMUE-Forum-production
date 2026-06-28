using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.Notification.Infrastructure.Services.Notifications;
using Xunit;

namespace UniHub.Notification.Infrastructure.Tests.Services.Notifications;

/// <summary>
/// Unit tests for WebPushNotificationService.
/// Note: Comprehensive testing of WebPushClient interaction requires mocking the underlying HTTP client,
/// which is complex. These tests focus on validation logic and basic service structure.
/// Integration tests with actual push subscriptions should be performed separately.
/// </summary>
public class WebPushNotificationServiceTests
{
    private readonly WebPushNotificationService _service;
    private readonly ILogger<WebPushNotificationService> _logger;
    private readonly WebPushSettings _settings;

    public WebPushNotificationServiceTests()
    {
        _logger = Substitute.For<ILogger<WebPushNotificationService>>();
        _settings = new WebPushSettings
        {
            Subject = "mailto:test@example.com",
            PublicKey = "BTestPublicKey",
            PrivateKey = "TestPrivateKey",
            MaxRetryAttempts = 3,
            TimeoutSeconds = 30
        };

        var options = Options.Create(_settings);
        _service = new WebPushNotificationService(options, _logger);
    }

    [Fact]
    public void Channel_ShouldReturnPush()
    {
        _service.Channel.Should().Be(NotificationChannel.Push);
    }

    [Fact]
    public async Task SendPushAsync_WithEmptyEndpoint_ShouldReturnFailure()
    {
        var result = await _service.SendPushAsync(
            string.Empty,
            "p256dh",
            "auth",
            "title",
            "body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WebPush.EndpointRequired");
    }

    [Fact]
    public async Task SendPushAsync_WithEmptyP256dh_ShouldReturnFailure()
    {
        var result = await _service.SendPushAsync(
            "https://endpoint.com",
            string.Empty,
            "auth",
            "title",
            "body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WebPush.P256dhRequired");
    }

    [Fact]
    public async Task SendPushAsync_WithEmptyAuth_ShouldReturnFailure()
    {
        var result = await _service.SendPushAsync(
            "https://endpoint.com",
            "p256dh",
            string.Empty,
            "title",
            "body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WebPush.AuthRequired");
    }

    [Fact]
    public async Task SendAsync_WithNotification_ShouldReturnNotImplementedError()
    {
        // Arrange
        var content = NotificationContent.Create(
            "Test Subject",
            "Test Body",
            "/action",
            "https://icon.url").Value;

        var notification = Domain.Notifications.Notification.Create(
            Guid.NewGuid(),
            NotificationChannel.Push,
            content).Value;

        // Act
        var result = await _service.SendAsync(notification);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("WebPush.NotImplemented");
        result.Error.Message.Should().Contain("Push subscription repository not yet implemented");
    }

    // TODO: Add integration tests with actual WebPush subscriptions
    // TODO: Add tests for retry logic with mocked WebPushClient
    // TODO: Add tests for expired/invalid subscription handling
}
