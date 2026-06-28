using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Strongly-typed identifier for NotificationTemplate aggregate.
/// </summary>
public sealed record NotificationTemplateId(Guid Value) : GuidId(Value)
{
    public static NotificationTemplateId Create(Guid value) => new(value);
    public static NotificationTemplateId CreateUnique() => new(Guid.NewGuid());
}
