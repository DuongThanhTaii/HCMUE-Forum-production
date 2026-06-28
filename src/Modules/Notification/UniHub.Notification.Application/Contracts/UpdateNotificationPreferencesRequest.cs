namespace UniHub.Notification.Application.Contracts;

/// <summary>
/// Request to update notification preferences.
/// </summary>
public sealed record UpdateNotificationPreferencesRequest(
    bool EmailEnabled,
    bool PushEnabled,
    bool InAppEnabled);
