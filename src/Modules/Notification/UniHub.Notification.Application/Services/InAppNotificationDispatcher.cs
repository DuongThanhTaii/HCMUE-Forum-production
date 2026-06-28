using Microsoft.Extensions.Logging;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Domain.Notifications;
using UniHub.Notification.Domain.NotificationTemplates;

namespace UniHub.Notification.Application.Services;

/// <summary>
/// Persists in-app notifications and pushes them over SignalR.
/// </summary>
public sealed class InAppNotificationDispatcher
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationPusher _pusher;
    private readonly ILogger<InAppNotificationDispatcher> _logger;

    public InAppNotificationDispatcher(
        INotificationRepository notificationRepository,
        INotificationPusher pusher,
        ILogger<InAppNotificationDispatcher> logger)
    {
        _notificationRepository = notificationRepository;
        _pusher = pusher;
        _logger = logger;
    }

    public async Task SendAsync(
        Guid recipientId,
        string subject,
        string body,
        string type,
        string? actionUrl = null,
        CancellationToken cancellationToken = default)
    {
        if (recipientId == Guid.Empty)
        {
            return;
        }

        try
        {
            var contentResult = NotificationContent.Create(subject, body, actionUrl);
            if (contentResult.IsFailure)
            {
                _logger.LogWarning("Notification content invalid: {Error}", contentResult.Error.Message);
                return;
            }

            var notifResult = Domain.Notifications.Notification.Create(
                recipientId,
                NotificationChannel.InApp,
                contentResult.Value);

            if (notifResult.IsFailure)
            {
                _logger.LogWarning("Notification create failed: {Error}", notifResult.Error.Message);
                return;
            }

            var notif = notifResult.Value;
            await _notificationRepository.AddAsync(notif, cancellationToken);

            await _pusher.PushAsync(
                recipientId,
                notif.Id.Value,
                subject,
                body,
                type,
                notif.CreatedAt,
                actionUrl,
                cancellationToken);

            var unreadCount = await _notificationRepository.GetUnreadCountAsync(recipientId, cancellationToken);
            await _pusher.PushUnreadCountAsync(recipientId, unreadCount, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to dispatch in-app notification to {RecipientId}", recipientId);
        }
    }

    public async Task SendToManyAsync(
        IEnumerable<Guid> recipientIds,
        string subject,
        string body,
        string type,
        string? actionUrl = null,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        var sent = new HashSet<Guid>();
        foreach (var recipientId in recipientIds)
        {
            if (recipientId == Guid.Empty || recipientId == excludeUserId || !sent.Add(recipientId))
            {
                continue;
            }

            await SendAsync(recipientId, subject, body, type, actionUrl, cancellationToken);
        }
    }
}
