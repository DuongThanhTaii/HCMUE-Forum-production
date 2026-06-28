namespace UniHub.Notification.Domain.NotificationTemplates;

/// <summary>
/// Category of notification for grouping and organization.
/// </summary>
public enum NotificationCategory
{
    /// <summary>Account-related notifications (registration, verification, password reset).</summary>
    Account = 1,

    /// <summary>Social/community activity (comments, posts, follows).</summary>
    Social = 2,

    /// <summary>Learning module activity (course enrollment, document approval).</summary>
    Learning = 3,

    /// <summary>Job/career-related notifications (applications, job postings).</summary>
    Career = 4,

    /// <summary>Direct messages and chat.</summary>
    Messaging = 5,

    /// <summary>System announcements and updates.</summary>
    System = 6
}
