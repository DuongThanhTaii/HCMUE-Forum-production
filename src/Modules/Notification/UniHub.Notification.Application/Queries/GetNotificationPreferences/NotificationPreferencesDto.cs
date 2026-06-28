namespace UniHub.Notification.Application.Queries.GetNotificationPreferences;

/// <summary>
/// DTO for notification preferences.
/// </summary>
public sealed record NotificationPreferencesDto(
    Guid UserId,
    bool EmailEnabled,
    bool PushEnabled,
    bool InAppEnabled,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
