namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Lifecycle status of a notification template.
/// </summary>
public enum NotificationTemplateStatus
{
    /// <summary>Template is being drafted and not used.</summary>
    Draft = 1,

    /// <summary>Template is active and in use.</summary>
    Active = 2,

    /// <summary>Template is archived and no longer in use.</summary>
    Archived = 3
}
