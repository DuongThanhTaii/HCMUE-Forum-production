using UniHub.SharedKernel.CQRS;

namespace UniHub.Notification.Application.Queries.GetNotificationPreferences;

/// <summary>
/// Query to get notification preferences for a user.
/// </summary>
public sealed record GetNotificationPreferencesQuery(Guid UserId) : IQuery<NotificationPreferencesDto>;
