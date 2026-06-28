namespace UniHub.Notification.Application.Abstractions.Notifications;

/// <summary>
/// Abstraction for pushing real-time notification payloads to connected clients.
/// Implemented in the Presentation layer via SignalR IHubContext.
/// </summary>
public interface INotificationPusher
{
    /// <summary>
    /// Push a notification to a specific user. Fire-and-forget; no throw on failure.
    /// </summary>
    Task PushAsync(
        Guid recipientId,
        Guid notificationId,
        string title,
        string message,
        string type,
        DateTime createdAt,
        string? actionUrl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Push updated unread count badge to a specific user.
    /// </summary>
    Task PushUnreadCountAsync(Guid recipientId, int count, CancellationToken cancellationToken = default);
}
