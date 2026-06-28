using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.NotificationPreferences;

/// <summary>
/// Domain errors for NotificationPreference aggregate.
/// </summary>
public static class NotificationPreferenceErrors
{
    public static readonly Error NotFound = new(
        "NotificationPreference.NotFound",
        "Notification preference not found");

    public static readonly Error UserIdEmpty = new(
        "NotificationPreference.UserIdEmpty",
        "User ID cannot be empty");

    public static readonly Error AlreadyExists = new(
        "NotificationPreference.AlreadyExists",
        "Notification preference already exists for this user");
}
