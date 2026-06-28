using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.NotificationPreferences.Events;

/// <summary>
/// Domain event raised when notification preferences are updated.
/// </summary>
/// <param name="PreferenceId">The preference ID.</param>
/// <param name="UserId">The user ID.</param>
/// <param name="EmailEnabled">Whether email is enabled.</param>
/// <param name="PushEnabled">Whether push is enabled.</param>
/// <param name="InAppEnabled">Whether in-app is enabled.</param>
/// <param name="UpdatedAt">When the preference was updated.</param>
public sealed record NotificationPreferenceUpdatedEvent(
    Guid PreferenceId,
    Guid UserId,
    bool EmailEnabled,
    bool PushEnabled,
    bool InAppEnabled,
    DateTime UpdatedAt) : IDomainEvent;
