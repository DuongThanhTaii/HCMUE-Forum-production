namespace UniHub.Notification.Domain.Notifications;

/// <summary>
/// Status of a notification in its lifecycle.
/// </summary>
public enum NotificationStatus
{
    /// <summary>Notification created but not yet sent.</summary>
    Pending = 1,

    /// <summary>Notification has been successfully sent.</summary>
    Sent = 2,

    /// <summary>Notification failed to send.</summary>
    Failed = 3,

    /// <summary>Notification has been read by the recipient.</summary>
    Read = 4,

    /// <summary>Notification has been dismissed by the recipient.</summary>
    Dismissed = 5
}
