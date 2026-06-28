using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.NotificationTemplates.Events;

/// <summary>
/// Raised when a notification template is activated.
/// </summary>
public sealed record NotificationTemplateActivatedEvent(
    Guid TemplateId,
    string Name,
    Guid ActivatedBy,
    DateTime ActivatedAt) : IDomainEvent;
