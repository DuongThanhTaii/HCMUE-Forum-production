using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Infrastructure.Services.Notifications;

/// <summary>
/// In-app notification service implementation.
/// In-app notifications are stored in database and displayed within the application UI.
/// No external service integration is required - notifications are persisted via repository.
/// </summary>
public sealed class InAppNotificationService : IInAppNotificationService
{
    private readonly ILogger<InAppNotificationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InAppNotificationService"/> class.
    /// </summary>
    /// <param name="logger">Logger instance.</param>
    public InAppNotificationService(ILogger<InAppNotificationService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public NotificationChannel Channel => NotificationChannel.InApp;

    /// <inheritdoc />
    public async Task<Result> SendAsync(
        Domain.Notifications.Notification notification,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Processing in-app notification {NotificationId} for recipient {RecipientId}",
                notification.Id.Value,
                notification.RecipientId);

            // In-app notifications don't require "sending" to external service
            // They are stored in database via repository and displayed in app UI
            
            // The notification aggregate is already created and will be persisted by repository
            // This service marks it as "sent" since there's no external delivery needed
            
            // In a real implementation, you might want to:
            // 1. Emit a SignalR event to notify connected clients in real-time
            // 2. Store notification in repository (handled by application layer)
            // 3. Update notification status to Sent

            _logger.LogInformation(
                "In-app notification {NotificationId} ready for display",
                notification.Id.Value);

            // Mark as success - the notification exists in domain and will be persisted
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to process in-app notification {NotificationId}",
                notification.Id.Value);

            return Result.Failure(
                new Error(
                    "InApp.ProcessFailed",
                    $"Failed to process in-app notification: {ex.Message}"));
        }
    }
}
