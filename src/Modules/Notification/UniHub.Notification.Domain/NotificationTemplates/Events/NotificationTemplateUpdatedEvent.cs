using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.NotificationTemplates.Events;

/// <summary>
/// Raised when a notification template content is updated.
/// </summary>
public sealed record NotificationTemplateUpdatedEvent(
    Guid TemplateId,
    string Name,
    Guid UpdatedBy,
    DateTime UpdatedAt) : IDomainEvent;
