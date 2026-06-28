using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Repositories;

/// <summary>
/// Repository for conversation operations
/// </summary>
public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Conversation>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    Task<Conversation?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task<Conversation> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConversationMessage> AddMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default);
    Task<ConversationMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default);
    Task<ConversationMessage> UpdateMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default);
}
