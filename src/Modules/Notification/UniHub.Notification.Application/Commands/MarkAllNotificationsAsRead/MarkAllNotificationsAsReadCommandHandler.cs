using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Commands.MarkAllNotificationsAsRead;

/// <summary>
/// Handler for marking all notifications as read.
/// </summary>
public sealed class MarkAllNotificationsAsReadCommandHandler : ICommandHandler<MarkAllNotificationsAsReadCommand, int>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<MarkAllNotificationsAsReadCommandHandler> _logger;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationRepository notificationRepository,
        ILogger<MarkAllNotificationsAsReadCommandHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var count = await _notificationRepository.MarkAllAsReadAsync(request.UserId, cancellationToken);

        _logger.LogInformation(
            "Marked {Count} notifications as read for user {UserId}",
            count,
            request.UserId);

        return Result.Success(count);
    }
}
