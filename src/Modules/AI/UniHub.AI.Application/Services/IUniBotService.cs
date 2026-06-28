using UniHub.AI.Application.DTOs;

namespace UniHub.AI.Application.Services;

/// <summary>
/// UniBot chatbot service that combines FAQ knowledge base with AI
/// </summary>
public interface IUniBotService
{
    /// <summary>
    /// Chat with UniBot
    /// </summary>
    Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get conversation history
    /// </summary>
    Task<ConversationDto?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get user's conversations
    /// </summary>
    Task<List<ConversationDto>> GetUserConversationsAsync(Guid userId, int skip = 0, int take = 20, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Close conversation
    /// </summary>
    Task<bool> CloseConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Mark response as helpful
    /// </summary>
    Task MarkResponseHelpfulAsync(Guid messageId, bool isHelpful, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Request handoff to human support
    /// </summary>
    Task<bool> RequestHandoffAsync(Guid conversationId, string reason, CancellationToken cancellationToken = default);
}
