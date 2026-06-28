using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.Notifications.Events;

/// <summary>
/// Raised when a notification is dismissed by the recipient.
/// </summary>
public sealed record NotificationDismissedEvent(
    Guid NotificationId,
    Guid RecipientId,
    DateTime DismissedAt) : IDomainEvent;
