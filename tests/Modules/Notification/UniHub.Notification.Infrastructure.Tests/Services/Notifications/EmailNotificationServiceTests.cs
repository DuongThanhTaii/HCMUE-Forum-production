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
/// Unit tests for EmailNotificationService.
/// Note: Comprehensive testing of SMTP client interaction requires integration tests.
/// These tests focus on validation logic and basic service structure.
/// </summary>
public class EmailNotificationServiceTests
{
    private readonly EmailNotificationService _service;
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly EmailSettings _settings;

    public EmailNotificationServiceTests()
    {
        _logger = Substitute.For<ILogger<EmailNotificationService>>();
        _settings = new EmailSettings
        {
            SmtpHost = "smtp.test.com",
            SmtpPort = 587,
            Username = "test@test.com",
            Password = "password",
            FromEmail = "noreply@test.com",
            FromName = "Test",
            EnableSsl = true,
            TimeoutSeconds = 30,
            MaxRetryAttempts = 3
        };

        var options = Options.Create(_settings);
        _service = new EmailNotificationService(options, _logger);
    }

    [Fact]
    public void Channel_ShouldReturnEmail()
    {
        _service.Channel.Should().Be(NotificationChannel.Email);
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyToEmail_ShouldReturnFailure()
    {
        var result = await _service.SendEmailAsync(
            string.Empty,
            "subject",
            "body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.ToEmailRequired");
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptySubject_ShouldReturnFailure()
    {
        var result = await _service.SendEmailAsync(
            "test@test.com",
            string.Empty,
            "body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.SubjectRequired");
    }

    [Fact]
    public async Task SendEmailAsync_WithEmptyBody_ShouldReturnFailure()
    {
        var result = await _service.SendEmailAsync(
            "test@test.com",
            "subject",
            string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.BodyRequired");
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
            NotificationChannel.Email,
            content).Value;

        // Act
        var result = await _service.SendAsync(notification);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.NotImplemented");
        result.Error.Message.Should().Contain("User repository not yet implemented");
    }

    // TODO: Add integration tests with actual SMTP server
    // TODO: Add tests for retry logic with mocked SmtpClient
    // TODO: Add tests for HTML vs plain text email
}
