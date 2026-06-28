using Microsoft.EntityFrameworkCore;
using UniHub.Infrastructure.Persistence;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Domain.Notifications;

namespace UniHub.Notification.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of INotificationRepository
/// </summary>
public sealed class NotificationRepository : INotificationRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Notifications.Notification?> GetByIdAsync(
        NotificationId id, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);
    }

    public async Task<(List<Domain.Notifications.Notification> Notifications, int TotalCount)> GetByRecipientAsync(
        Guid recipientId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .Where(n => n.RecipientId == recipientId)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> GetUnreadCountAsync(Guid recipientId, CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .CountAsync(
                n => n.RecipientId == recipientId &&
                     n.Status == NotificationStatus.Sent &&
                     n.ReadAt == null,
                cancellationToken);
    }

    public async Task AddAsync(Domain.Notifications.Notification notification, CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public Task UpdateAsync(Domain.Notifications.Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Update(notification);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Notifications.Notification notification, CancellationToken cancellationToken = default)
    {
        _context.Notifications.Remove(notification);
        return Task.CompletedTask;
    }

    public async Task<int> MarkAllAsReadAsync(Guid recipientId, CancellationToken cancellationToken = default)
    {
        var unread = await _context.Notifications
            .Where(n => n.RecipientId == recipientId &&
                        n.Status == NotificationStatus.Sent &&
                        n.ReadAt == null)
            .ToListAsync(cancellationToken);

        foreach (var notification in unread)
        {
            notification.MarkAsRead();
        }

        return unread.Count;
    }
}
