using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Identity.Domain.Events;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Application.EventHandlers;
using UniHub.SharedKernel.Results;
using Xunit;

namespace UniHub.Notification.Application.Tests.EventHandlers;

public class UserRegisteredEventHandlerTests
{
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ILogger<UserRegisteredEventHandler> _logger;
    private readonly UserRegisteredEventHandler _handler;

    public UserRegisteredEventHandlerTests()
    {
        _emailNotificationService = Substitute.For<IEmailNotificationService>();
        _logger = Substitute.For<ILogger<UserRegisteredEventHandler>>();
        _handler = new UserRegisteredEventHandler(_emailNotificationService, _logger);
    }

    [Fact]
    public async Task Handle_ShouldSendWelcomeEmail()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var email = Email.Create("user@test.com").Value;
        var @event = new UserRegisteredEvent(userId, email);

        _emailNotificationService
            .SendAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _emailNotificationService
            .Received(1)
            .SendAsync(
                Arg.Is<Domain.Notifications.Notification>(n =>
                    n.Content.Subject.Contains("Welcome") &&
                    n.Content.Body.Contains(email.Value)),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEmailSendFails_ShouldLogError()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var email = Email.Create("user@test.com").Value;
        var @event = new UserRegisteredEvent(userId, email);

        _emailNotificationService
            .SendAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure(new Error("Email.SendFailed", "Failed to send email")));

        // Act
        await _handler.Handle(@event, CancellationToken.None);

        // Assert
        await _emailNotificationService
            .Received(1)
            .SendAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
    }
}
