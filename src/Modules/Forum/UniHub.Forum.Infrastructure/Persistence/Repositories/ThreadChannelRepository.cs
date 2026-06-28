using Microsoft.EntityFrameworkCore;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.ThreadChannels;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Forum.Infrastructure.Persistence.Repositories;

public sealed class ThreadChannelRepository : IThreadChannelRepository
{
    private readonly ApplicationDbContext _context;

    public ThreadChannelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ThreadChannel>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default)
    {
        var query = _context.ThreadChannels.AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(x => x.IsActive);
        }

        var channels = await query
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return channels.AsReadOnly();
    }

    public async Task<ThreadChannel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ThreadChannels.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<ThreadChannel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToLowerInvariant();
        return await _context.ThreadChannels.FirstOrDefaultAsync(x => x.Code == normalized, cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default)
    {
        var normalized = code.Trim().ToLowerInvariant();
        return await _context.ThreadChannels.AnyAsync(
            x => x.Code == normalized && (!excludingId.HasValue || x.Id != excludingId.Value),
            cancellationToken);
    }

    public async Task AddAsync(ThreadChannel channel, CancellationToken cancellationToken = default)
    {
        await _context.ThreadChannels.AddAsync(channel, cancellationToken);
    }

    public Task UpdateAsync(ThreadChannel channel, CancellationToken cancellationToken = default)
    {
        _context.ThreadChannels.Update(channel);
        return Task.CompletedTask;
    }
}
