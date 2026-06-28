using UniHub.Notification.Domain.NotificationPreferences;

namespace UniHub.Notification.Application.Abstractions;

public interface INotificationPreferenceRepository
{
    Task<NotificationPreference?> GetByIdAsync(NotificationPreferenceId id, CancellationToken cancellationToken = default);

    Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(NotificationPreference preference, CancellationToken cancellationToken = default);

    Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default);

    Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
