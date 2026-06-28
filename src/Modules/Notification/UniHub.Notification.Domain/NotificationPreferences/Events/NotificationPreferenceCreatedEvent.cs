using UniHub.SharedKernel.Domain;

namespace UniHub.Notification.Domain.NotificationPreferences.Events;

/// <summary>
/// Domain event raised when a notification preference is created.
/// </summary>
/// <param name="PreferenceId">The preference ID.</param>
/// <param name="UserId">The user ID.</param>
/// <param name="CreatedAt">When the preference was created.</param>
public sealed record NotificationPreferenceCreatedEvent(
    Guid PreferenceId,
    Guid UserId,
    DateTime CreatedAt) : IDomainEvent;
