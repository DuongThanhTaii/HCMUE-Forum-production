using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Queries.GetNotifications;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using Xunit;

namespace UniHub.Notification.Application.Tests.Queries;

public class GetNotificationsQueryHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<GetNotificationsQueryHandler> _logger;
    private readonly GetNotificationsQueryHandler _handler;

    public GetNotificationsQueryHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _logger = Substitute.For<ILogger<GetNotificationsQueryHandler>>();
        _handler = new GetNotificationsQueryHandler(_notificationRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ShouldReturnPaginatedNotifications()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var contentResult = NotificationContent.Create("Test", "Body", "/test");
        var notification1 = Domain.Notifications.Notification.Create(
            userId,
            NotificationChannel.Email,
            contentResult.Value).Value;
        var notification2 = Domain.Notifications.Notification.Create(
            userId,
            NotificationChannel.Push,
            contentResult.Value).Value;

        var notifications = new List<Domain.Notifications.Notification> { notification1, notification2 };
        _notificationRepository.GetByRecipientAsync(userId, 1, 20, Arg.Any<CancellationToken>())
            .Returns((notifications, 2));

        var query = new GetNotificationsQuery(userId, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Notifications.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WhenInvalidPageNumber_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetNotificationsQuery(userId, 0, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GetNotifications.InvalidPageNumber");
    }

    [Fact]
    public async Task Handle_WhenInvalidPageSize_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetNotificationsQuery(userId, 1, 101);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GetNotifications.InvalidPageSize");
    }
}
