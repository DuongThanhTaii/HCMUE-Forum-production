using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.Notifications;

/// <summary>
/// Strongly-typed identifier for Notification aggregate.
/// </summary>
public sealed record NotificationId(Guid Value) : GuidId(Value)
{
    public static NotificationId Create(Guid value) => new(value);
    public static NotificationId CreateUnique() => new(Guid.NewGuid());
}
