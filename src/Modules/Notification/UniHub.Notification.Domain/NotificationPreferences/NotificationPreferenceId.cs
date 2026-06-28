using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.NotificationPreferences;

/// <summary>
/// Strongly-typed identifier for NotificationPreference aggregate.
/// </summary>
public sealed record NotificationPreferenceId(Guid Value) : GuidId(Value)
{
    /// <summary>
    /// Creates a new NotificationPreferenceId from a Guid value.
    /// </summary>
    public static NotificationPreferenceId Create(Guid value) => new(value);

    /// <summary>
    /// Creates a new unique NotificationPreferenceId.
    /// </summary>
    public static NotificationPreferenceId CreateUnique() => new(Guid.NewGuid());
}
