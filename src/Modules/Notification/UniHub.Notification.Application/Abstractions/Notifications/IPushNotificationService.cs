using UniHub.Notification.Domain.Notifications;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Application.Abstractions.Notifications;

/// <summary>
/// Service for sending push notifications.
/// </summary>
public interface IPushNotificationService : INotificationSender
{
    /// <summary>
    /// Sends a push notification to a specific device using subscription endpoint.
    /// </summary>
    /// <param name="endpoint">The push subscription endpoint URL.</param>
    /// <param name="p256dh">The P256DH key from the push subscription.</param>
    /// <param name="auth">The Auth key from the push subscription.</param>
    /// <param name="title">Notification title.</param>
    /// <param name="body">Notification body.</param>
    /// <param name="icon">Optional icon URL.</param>
    /// <param name="url">Optional action URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result> SendPushAsync(
        string endpoint,
        string p256dh,
        string auth,
        string title,
        string body,
        string? icon = null,
        string? url = null,
        CancellationToken cancellationToken = default);
}
