using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.Notifications.Events;

/// <summary>
/// Raised when a notification is read by the recipient.
/// </summary>
public sealed record NotificationReadEvent(
    Guid NotificationId,
    Guid RecipientId,
    DateTime ReadAt) : IDomainEvent;
