using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;

namespace UniHub.Chat.Application.Abstractions;

/// <summary>
/// Repository for Conversation aggregate operations
/// </summary>
public interface IConversationRepository
{
    /// <summary>
    /// Get a conversation by ID
    /// </summary>
    Task<Conversation?> GetByIdAsync(ConversationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all conversations for a user
    /// </summary>
    Task<IReadOnlyList<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a direct conversation between two users
    /// </summary>
    Task<Conversation?> GetDirectConversationAsync(Guid user1Id, Guid user2Id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a conversation exists
    /// </summary>
    Task<bool> ExistsAsync(ConversationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Whether the user is in the conversation participants JSON (jsonb-safe query).
    /// </summary>
    Task<bool> IsUserParticipantAsync(
        ConversationId conversationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new conversation
    /// </summary>
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing conversation
    /// </summary>
    Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);
}
