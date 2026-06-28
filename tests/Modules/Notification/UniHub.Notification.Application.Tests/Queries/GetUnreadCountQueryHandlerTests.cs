using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Queries.GetUnreadCount;
using Xunit;

namespace UniHub.Notification.Application.Tests.Queries;

public class GetUnreadCountQueryHandlerTests
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<GetUnreadCountQueryHandler> _logger;
    private readonly GetUnreadCountQueryHandler _handler;

    public GetUnreadCountQueryHandlerTests()
    {
        _notificationRepository = Substitute.For<INotificationRepository>();
        _logger = Substitute.For<ILogger<GetUnreadCountQueryHandler>>();
        _handler = new GetUnreadCountQueryHandler(_notificationRepository, _logger);
    }

    [Fact]
    public async Task Handle_ShouldReturnUnreadCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _notificationRepository.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(5);

        var query = new GetUnreadCountQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WhenNoUnreadNotifications_ShouldReturnZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _notificationRepository.GetUnreadCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new GetUnreadCountQuery(userId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }
}
