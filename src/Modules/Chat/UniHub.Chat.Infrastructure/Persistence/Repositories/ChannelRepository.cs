using Microsoft.EntityFrameworkCore;
using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Channels;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Chat.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of channel repository for Chat module
/// </summary>
public sealed class ChannelRepository : IChannelRepository
{
    /// <summary>Backing field for JSONB members array — must use in LINQ instead of <see cref="Channel.Members"/>.</summary>
    private const string MembersBackingField = "_members";

    private readonly ApplicationDbContext _context;

    public ChannelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Channel?> GetByIdAsync(ChannelId id, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Channel>> GetPublicChannelsAsync(CancellationToken cancellationToken = default)
    {
        var publicChannels = await _context.Channels
            .Where(c => c.Type == ChannelType.Public && !c.IsArchived)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return publicChannels.AsReadOnly();
    }

    public async Task<IReadOnlyList<Channel>> GetByMemberIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var memberChannels = await _context.Channels
            .Where(c => EF.Property<List<Guid>>(c, MembersBackingField).Contains(userId) && !c.IsArchived)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return memberChannels.AsReadOnly();
    }

    public async Task<bool> ExistsAsync(ChannelId id, CancellationToken cancellationToken = default)
    {
        return await _context.Channels
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AddAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        await _context.Channels.AddAsync(channel, cancellationToken);
    }

    public Task UpdateAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        _context.Channels.Update(channel);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Channel channel, CancellationToken cancellationToken = default)
    {
        _context.Channels.Remove(channel);
        return Task.CompletedTask;
    }
}
