using Microsoft.EntityFrameworkCore;
using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Chat.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of conversation repository for Chat module
/// </summary>
public sealed class ConversationRepository : IConversationRepository
{
    private readonly ApplicationDbContext _context;

    public ConversationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(ConversationId id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var participantJson = ConversationParticipantQuery.ContainsPayload(userId);

        var conversations = await _context.Conversations
            .FromSqlInterpolated($"""
                SELECT id, type, title, created_by, created_at, last_message_at, is_archived, participants
                FROM chat.conversations
                WHERE NOT is_archived
                  AND participants @> ({participantJson})::jsonb
                ORDER BY COALESCE(last_message_at, created_at) DESC
                """)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return conversations.AsReadOnly();
    }

    public async Task<Conversation?> GetDirectConversationAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default)
    {
        var user1Json = ConversationParticipantQuery.ContainsPayload(user1Id);
        var user2Json = ConversationParticipantQuery.ContainsPayload(user2Id);

        return await _context.Conversations
            .FromSqlInterpolated($"""
                SELECT id, type, title, created_by, created_at, last_message_at, is_archived, participants
                FROM chat.conversations
                WHERE type = {(int)ConversationType.Direct}
                  AND participants @> ({user1Json})::jsonb
                  AND participants @> ({user2Json})::jsonb
                LIMIT 1
                """)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(ConversationId id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> IsUserParticipantAsync(
        ConversationId conversationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var participantJson = ConversationParticipantQuery.ContainsPayload(userId);

        return await _context.Conversations
            .FromSqlInterpolated($"""
                SELECT id, type, title, created_by, created_at, last_message_at, is_archived, participants
                FROM chat.conversations
                WHERE id = {conversationId.Value}
                  AND participants @> ({participantJson})::jsonb
                """)
            .AsNoTracking()
            .AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _context.Conversations.AddAsync(conversation, cancellationToken);
    }

    public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _context.Conversations.Update(conversation);
        return Task.CompletedTask;
    }
}
