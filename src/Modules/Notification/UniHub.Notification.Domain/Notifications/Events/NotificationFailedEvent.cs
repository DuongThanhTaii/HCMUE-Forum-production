using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.Notifications.Events;

/// <summary>
/// Raised when a notification fails to send.
/// </summary>
public sealed record NotificationFailedEvent(
    Guid NotificationId,
    Guid RecipientId,
    string FailureReason,
    DateTime FailedAt) : IDomainEvent;
