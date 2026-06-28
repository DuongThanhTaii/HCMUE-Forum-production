using Microsoft.EntityFrameworkCore;
using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Safety;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Chat.Infrastructure.Persistence.Repositories;

public sealed class ConversationMuteRepository : IConversationMuteRepository
{
    private readonly ApplicationDbContext _context;

    public ConversationMuteRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<ConversationMute?> GetAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default) =>
        _context.ConversationMutes.FirstOrDefaultAsync(
            m => m.UserId == userId && m.ConversationId == conversationId,
            cancellationToken);

    public async Task<IReadOnlyDictionary<Guid, bool>> GetMuteStatesAsync(
        Guid userId,
        IReadOnlyList<Guid> conversationIds,
        CancellationToken cancellationToken = default)
    {
        if (conversationIds.Count == 0)
        {
            return new Dictionary<Guid, bool>();
        }

        var rows = await _context.ConversationMutes
            .AsNoTracking()
            .Where(m => m.UserId == userId && conversationIds.Contains(m.ConversationId))
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(m => m.ConversationId, m => m.IsMuted);
    }

    public async Task UpsertAsync(ConversationMute mute, CancellationToken cancellationToken = default)
    {
        var existing = await GetAsync(mute.UserId, mute.ConversationId, cancellationToken);
        if (existing is null)
        {
            await _context.ConversationMutes.AddAsync(mute, cancellationToken);
            return;
        }

        existing.SetMuted(mute.IsMuted);
        _context.ConversationMutes.Update(existing);
    }
}
