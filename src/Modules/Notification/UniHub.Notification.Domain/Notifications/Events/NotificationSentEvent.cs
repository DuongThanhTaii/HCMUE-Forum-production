using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.Notifications.Events;

/// <summary>
/// Raised when a notification is successfully sent.
/// </summary>
public sealed record NotificationSentEvent(
    Guid NotificationId,
    Guid RecipientId,
    DateTime SentAt) : IDomainEvent;
