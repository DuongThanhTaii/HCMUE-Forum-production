namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Notification delivery channel.
/// </summary>
public enum NotificationChannel
{
    /// <summary>Email notification.</summary>
    Email = 1,

    /// <summary>Web push notification.</summary>
    Push = 2,

    /// <summary>In-app notification.</summary>
    InApp = 3
}
