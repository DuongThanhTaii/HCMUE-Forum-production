using UniHub.SharedKernel.CQRS;

namespace UniHub.Notification.Application.Commands.UpdateNotificationPreferences;

/// <summary>
/// Command to update notification preferences for a user.
/// </summary>
public sealed record UpdateNotificationPreferencesCommand(
    Guid UserId,
    bool EmailEnabled,
    bool PushEnabled,
    bool InAppEnabled) : ICommand;
