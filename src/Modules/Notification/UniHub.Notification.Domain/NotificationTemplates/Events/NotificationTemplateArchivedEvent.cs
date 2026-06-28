using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.NotificationTemplates.Events;

/// <summary>
/// Raised when a notification template is archived.
/// </summary>
public sealed record NotificationTemplateArchivedEvent(
    Guid TemplateId,
    string Name,
    Guid ArchivedBy,
    DateTime ArchivedAt) : IDomainEvent;
