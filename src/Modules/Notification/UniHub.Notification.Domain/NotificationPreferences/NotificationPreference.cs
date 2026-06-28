using UniHub.Notification.Domain.NotificationPreferences.Events;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Notification.Domain.NotificationPreferences;

/// <summary>
/// NotificationPreference aggregate root.
/// Manages user preferences for different notification channels.
/// Each user has one preference record controlling Email, Push, and InApp notifications.
/// </summary>
public sealed class NotificationPreference : AggregateRoot<NotificationPreferenceId>
{
    /// <summary>
    /// User ID this preference belongs to.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Whether email notifications are enabled.
    /// </summary>
    public bool EmailEnabled { get; private set; }

    /// <summary>
    /// Whether push notifications are enabled.
    /// </summary>
    public bool PushEnabled { get; private set; }

    /// <summary>
    /// Whether in-app notifications are enabled.
    /// </summary>
    public bool InAppEnabled { get; private set; }

    /// <summary>
    /// When the preference was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// When the preference was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for EF Core
    private NotificationPreference() { }

    /// <summary>
    /// Private constructor for creating new instances.
    /// </summary>
    private NotificationPreference(
        NotificationPreferenceId id,
        Guid userId,
        bool emailEnabled,
        bool pushEnabled,
        bool inAppEnabled)
        : base(id)
    {
        UserId = userId;
        EmailEnabled = emailEnabled;
        PushEnabled = pushEnabled;
        InAppEnabled = inAppEnabled;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a new NotificationPreference with default settings (all channels enabled).
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <returns>Result containing the new NotificationPreference or error.</returns>
    public static Result<NotificationPreference> Create(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<NotificationPreference>(
                NotificationPreferenceErrors.UserIdEmpty);
        }

        var preference = new NotificationPreference(
            NotificationPreferenceId.CreateUnique(),
            userId,
            emailEnabled: true,
            pushEnabled: true,
            inAppEnabled: true);

        preference.AddDomainEvent(new NotificationPreferenceCreatedEvent(
            preference.Id.Value,
            preference.UserId,
            preference.CreatedAt));

        return Result.Success(preference);
    }

    /// <summary>
    /// Updates email notification preference.
    /// </summary>
    /// <param name="enabled">Whether email notifications should be enabled.</param>
    public void UpdateEmailPreference(bool enabled)
    {
        if (EmailEnabled == enabled)
        {
            return; // No change
        }

        EmailEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferenceUpdatedEvent(
            Id.Value,
            UserId,
            EmailEnabled,
            PushEnabled,
            InAppEnabled,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Updates push notification preference.
    /// </summary>
    /// <param name="enabled">Whether push notifications should be enabled.</param>
    public void UpdatePushPreference(bool enabled)
    {
        if (PushEnabled == enabled)
        {
            return; // No change
        }

        PushEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferenceUpdatedEvent(
            Id.Value,
            UserId,
            EmailEnabled,
            PushEnabled,
            InAppEnabled,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Updates in-app notification preference.
    /// </summary>
    /// <param name="enabled">Whether in-app notifications should be enabled.</param>
    public void UpdateInAppPreference(bool enabled)
    {
        if (InAppEnabled == enabled)
        {
            return; // No change
        }

        InAppEnabled = enabled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferenceUpdatedEvent(
            Id.Value,
            UserId,
            EmailEnabled,
            PushEnabled,
            InAppEnabled,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Updates all notification preferences at once.
    /// </summary>
    public void UpdateAll(bool emailEnabled, bool pushEnabled, bool inAppEnabled)
    {
        var changed = EmailEnabled != emailEnabled ||
                     PushEnabled != pushEnabled ||
                     InAppEnabled != inAppEnabled;

        if (!changed)
        {
            return; // No changes
        }

        EmailEnabled = emailEnabled;
        PushEnabled = pushEnabled;
        InAppEnabled = inAppEnabled;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new NotificationPreferenceUpdatedEvent(
            Id.Value,
            UserId,
            EmailEnabled,
            PushEnabled,
            InAppEnabled,
            UpdatedAt.Value));
    }

    /// <summary>
    /// Enables all notification channels.
    /// </summary>
    public void EnableAll()
    {
        UpdateAll(emailEnabled: true, pushEnabled: true, inAppEnabled: true);
    }

    /// <summary>
    /// Disables all notification channels.
    /// </summary>
    public void DisableAll()
    {
        UpdateAll(emailEnabled: false, pushEnabled: false, inAppEnabled: false);
    }
}
