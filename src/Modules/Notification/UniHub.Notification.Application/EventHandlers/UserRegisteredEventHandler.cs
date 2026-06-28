using Microsoft.Extensions.Logging;
using UniHub.Identity.Domain.Events;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Application.EventHandlers;

/// <summary>
/// Handles UserRegisteredEvent by sending a welcome email to the new user.
/// </summary>
public sealed class UserRegisteredEventHandler : IDomainEventHandler<UserRegisteredEvent>
{
    private readonly IEmailNotificationService _emailNotificationService;
    private readonly ILogger<UserRegisteredEventHandler> _logger;

    public UserRegisteredEventHandler(
        IEmailNotificationService emailNotificationService,
        ILogger<UserRegisteredEventHandler> logger)
    {
        _emailNotificationService = emailNotificationService;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling UserRegisteredEvent for user {UserId} with email {Email}",
            notification.UserId.Value,
            notification.Email.Value);

        try
        {
            // Create notification content
            var contentResult = NotificationContent.Create(
                subject: "Welcome to UniHub!",
                body: $"Welcome {notification.Email.Value}! We're excited to have you join our community.",
                actionUrl: "/dashboard");

            if (contentResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to create notification content for user {UserId}: {Error}",
                    notification.UserId.Value,
                    contentResult.Error.Message);
                return;
            }

            // Create welcome notification
            var notificationResult = Domain.Notifications.Notification.Create(
                recipientId: notification.UserId.Value,
                channel: NotificationChannel.Email,
                content: contentResult.Value);

            if (notificationResult.IsFailure)
            {
                _logger.LogError(
                    "Failed to create welcome notification for user {UserId}: {Error}",
                    notification.UserId.Value,
                    notificationResult.Error.Message);
                return;
            }

            var notif = notificationResult.Value;

            // Send email notification
            var sendResult = await _emailNotificationService.SendAsync(notif, cancellationToken);

            if (sendResult.IsSuccess)
            {
                _logger.LogInformation(
                    "Successfully sent welcome email to user {UserId}",
                    notification.UserId.Value);
            }
            else
            {
                _logger.LogError(
                    "Failed to send welcome email to user {UserId}: {Error}",
                    notification.UserId.Value,
                    sendResult.Error.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error handling UserRegisteredEvent for user {UserId}",
                notification.UserId.Value);
        }
    }
}
