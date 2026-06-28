using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Commands.DeleteNotification;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using Xunit;

namespace UniHub.Notification.Application.Tests.Commands;

public class DeleteNotificationCommandHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<DeleteNotificationCommandHandler> _logger;
    private readonly DeleteNotificationCommandHandler _handler;

    public DeleteNotificationCommandHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _logger = Substitute.For<ILogger<DeleteNotificationCommandHandler>>();
        _handler = new DeleteNotificationCommandHandler(_notificationRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenNotificationExists_ShouldDelete()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var contentResult = NotificationContent.Create("Test", "Body", "/test");
        var notification = Domain.Notifications.Notification.Create(
            userId,
            NotificationChannel.Email,
            contentResult.Value).Value;

        var idProperty = typeof(Domain.Notifications.Notification).GetProperty("Id");
        idProperty?.SetValue(notification, NotificationId.Create(notificationId));

        _notificationRepository.GetByIdAsync(Arg.Any<NotificationId>(), Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new DeleteNotificationCommand(notificationId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _notificationRepository.Received(1).DeleteAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotificationNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        _notificationRepository.GetByIdAsync(Arg.Any<NotificationId>(), Arg.Any<CancellationToken>())
            .Returns((Domain.Notifications.Notification?)null);

        var command = new DeleteNotificationCommand(notificationId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.NotFound");
        await _notificationRepository.DidNotReceive().DeleteAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotificationBelongsToAnotherUser_ShouldReturnForbidden()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var requesterId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var contentResult = NotificationContent.Create("Test", "Body", "/test");
        var notification = Domain.Notifications.Notification.Create(
            ownerId,
            NotificationChannel.Email,
            contentResult.Value).Value;

        var idProperty = typeof(Domain.Notifications.Notification).GetProperty("Id");
        idProperty?.SetValue(notification, NotificationId.Create(notificationId));

        _notificationRepository.GetByIdAsync(Arg.Any<NotificationId>(), Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new DeleteNotificationCommand(notificationId, requesterId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.Forbidden");
        await _notificationRepository.DidNotReceive().DeleteAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
    }
}
