using UniHub.Notification.Domain.NotificationTemplates;
using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.Notifications.Events;

/// <summary>
/// Raised when a new notification is created.
/// </summary>
public sealed record NotificationCreatedEvent(
    Guid NotificationId,
    Guid RecipientId,
    NotificationChannel Channel,
    string Subject,
    DateTime CreatedAt) : IDomainEvent;
