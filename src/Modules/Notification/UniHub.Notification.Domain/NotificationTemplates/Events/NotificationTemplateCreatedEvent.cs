using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.NotificationTemplates.Events;

/// <summary>
/// Raised when a new notification template is created.
/// </summary>
public sealed record NotificationTemplateCreatedEvent(
    Guid TemplateId,
    string Name,
    string DisplayName,
    NotificationCategory Category,
    Guid CreatedBy,
    DateTime CreatedAt) : IDomainEvent;
