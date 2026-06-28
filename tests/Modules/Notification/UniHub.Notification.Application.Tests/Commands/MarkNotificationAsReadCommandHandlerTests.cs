using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Commands.MarkNotificationAsRead;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using Xunit;

namespace UniHub.Notification.Application.Tests.Commands;

public class MarkNotificationAsReadCommandHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<MarkNotificationAsReadCommandHandler> _logger;
    private readonly MarkNotificationAsReadCommandHandler _handler;

    public MarkNotificationAsReadCommandHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _logger = Substitute.For<ILogger<MarkNotificationAsReadCommandHandler>>();
        _handler = new MarkNotificationAsReadCommandHandler(_notificationRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenNotificationExists_ShouldMarkAsRead()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        var contentResult = NotificationContent.Create("Test", "Body", "/test");
        var notification = Domain.Notifications.Notification.Create(
            userId,
            NotificationChannel.Email,
            contentResult.Value).Value;

        // Use reflection to set the Id property since it's typically set by the infrastructure layer
        var idProperty = typeof(Domain.Notifications.Notification).GetProperty("Id");
        idProperty?.SetValue(notification, NotificationId.Create(notificationId));

        _notificationRepository.GetByIdAsync(Arg.Any<NotificationId>(), Arg.Any<CancellationToken>())
            .Returns(notification);

        var command = new MarkNotificationAsReadCommand(notificationId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        notification.IsRead().Should().BeTrue();
        await _notificationRepository.Received(1).UpdateAsync(notification, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotificationNotFound_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();
        _notificationRepository.GetByIdAsync(Arg.Any<NotificationId>(), Arg.Any<CancellationToken>())
            .Returns((Domain.Notifications.Notification?)null);

        var command = new MarkNotificationAsReadCommand(notificationId, userId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.NotFound");
        await _notificationRepository.DidNotReceive().UpdateAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
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

        var command = new MarkNotificationAsReadCommand(notificationId, requesterId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Notification.Forbidden");
        await _notificationRepository.DidNotReceive().UpdateAsync(Arg.Any<Domain.Notifications.Notification>(), Arg.Any<CancellationToken>());
    }
}
