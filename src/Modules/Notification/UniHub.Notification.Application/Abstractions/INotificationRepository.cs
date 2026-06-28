using UniHub.Notification.Domain.Notifications;
using NotificationEntity = UniHub.Notification.Domain.Notifications.Notification;

namespace UniHub.Notification.Application.Abstractions;

public interface INotificationRepository
{
    Task<NotificationEntity?> GetByIdAsync(NotificationId id, CancellationToken cancellationToken = default);

    Task<(List<NotificationEntity> Notifications, int TotalCount)> GetByRecipientAsync(
        Guid recipientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid recipientId, CancellationToken cancellationToken = default);

    Task AddAsync(NotificationEntity notification, CancellationToken cancellationToken = default);

    Task UpdateAsync(NotificationEntity notification, CancellationToken cancellationToken = default);

    Task DeleteAsync(NotificationEntity notification, CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(Guid recipientId, CancellationToken cancellationToken = default);
}
