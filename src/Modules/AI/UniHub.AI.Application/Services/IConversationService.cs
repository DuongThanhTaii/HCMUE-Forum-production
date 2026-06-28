using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Application.Services;

/// <summary>
/// Service for managing conversations
/// </summary>
public interface IConversationService
{
    /// <summary>
    /// Get conversation by ID
    /// </summary>
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get conversations for user
    /// </summary>
    Task<List<Conversation>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get conversation by session ID (for anonymous users)
    /// </summary>
    Task<Conversation?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create new conversation
    /// </summary>
    Task<Conversation> CreateAsync(Guid? userId, string? sessionId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Add message to conversation
    /// </summary>
    Task<ConversationMessage> AddMessageAsync(Guid conversationId, MessageRole role, string content, 
        Guid? sourceFAQId = null, double? confidenceScore = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Mark message as helpful/not helpful
    /// </summary>
    Task MarkMessageHelpfulAsync(Guid messageId, bool isHelpful, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Hand off conversation to human support
    /// </summary>
    Task<bool> HandoffToSupportAsync(Guid conversationId, string reason, Guid? supportAgentId = null, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Close conversation
    /// </summary>
    Task<bool> CloseConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete conversation
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
