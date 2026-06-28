using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Abstractions.Notifications;

/// <summary>
/// Base interface for all notification senders.
/// Each channel (Push, Email, InApp) implements this interface.
/// </summary>
public interface INotificationSender
{
    /// <summary>
    /// Sends a notification through the specific channel.
    /// </summary>
    /// <param name="notification">The notification to send.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure with error details.</returns>
    Task<Result> SendAsync(Domain.Notifications.Notification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the notification channel this sender handles.
    /// </summary>
    NotificationChannel Channel { get; }
}
