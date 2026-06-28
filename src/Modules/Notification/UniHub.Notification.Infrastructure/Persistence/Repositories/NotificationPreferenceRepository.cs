using Microsoft.EntityFrameworkCore;
using UniHub.Infrastructure.Persistence;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Domain.NotificationPreferences;

namespace UniHub.Notification.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of INotificationPreferenceRepository
/// </summary>
public sealed class NotificationPreferenceRepository : INotificationPreferenceRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationPreferenceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationPreference?> GetByIdAsync(
        NotificationPreferenceId id, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<NotificationPreference?> GetByUserIdAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        await _context.NotificationPreferences.AddAsync(preference, cancellationToken);
    }

    public Task UpdateAsync(NotificationPreference preference, CancellationToken cancellationToken = default)
    {
        _context.NotificationPreferences.Update(preference);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.NotificationPreferences
            .AnyAsync(p => p.UserId == userId, cancellationToken);
    }
}
