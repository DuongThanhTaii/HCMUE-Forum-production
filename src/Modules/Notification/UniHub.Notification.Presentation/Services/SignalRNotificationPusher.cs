using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Presentation.Hubs;

namespace UniHub.Notification.Presentation.Services;

/// <summary>
/// Pushes real-time notification payloads to clients via NotificationHub.
/// </summary>
public sealed class SignalRNotificationPusher : INotificationPusher
{
    private readonly IHubContext<NotificationHub, INotificationClient> _hub;
    private readonly ILogger<SignalRNotificationPusher> _logger;

    public SignalRNotificationPusher(
        IHubContext<NotificationHub, INotificationClient> hub,
        ILogger<SignalRNotificationPusher> logger)
    {
        _hub = hub;
        _logger = logger;
    }

    public async Task PushAsync(
        Guid recipientId,
        Guid notificationId,
        string title,
        string message,
        string type,
        DateTime createdAt,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Dictionary<string, object>? data = null;
            if (!string.IsNullOrWhiteSpace(actionUrl))
            {
                data = new Dictionary<string, object> { ["actionUrl"] = actionUrl };
            }

            var msg = new NotificationMessage(notificationId, title, message, type, createdAt, data);
            await _hub.Clients
                .Group($"user-{recipientId}")
                .ReceiveNotification(msg);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to push real-time notification {NotificationId} to user {UserId}",
                notificationId, recipientId);
        }
    }

    public async Task PushUnreadCountAsync(Guid recipientId, int count, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hub.Clients
                .Group($"user-{recipientId}")
                .UnreadCountUpdated(count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to push unread count to user {UserId}", recipientId);
        }
    }
}
