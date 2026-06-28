using UniHub.AI.Application.Services;
using UniHub.AI.Domain.Entities;
using UniHub.AI.Infrastructure.Repositories;

namespace UniHub.AI.Infrastructure.Services;

/// <summary>
/// Implementation of conversation service
/// </summary>
public class ConversationService : IConversationService
{
    private readonly IConversationRepository _repository;

    public ConversationService(IConversationRepository repository)
    {
        _repository = repository;
    }

    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public Task<List<Conversation>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        return _repository.GetByUserIdAsync(userId, skip, take, cancellationToken);
    }

    public Task<Conversation?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return _repository.GetBySessionIdAsync(sessionId, cancellationToken);
    }

    public Task<Conversation> CreateAsync(Guid? userId, string? sessionId, CancellationToken cancellationToken = default)
    {
        var conversation = new Conversation
        {
            UserId = userId,
            SessionId = sessionId
        };
        return _repository.CreateAsync(conversation, cancellationToken);
    }

    public async Task<ConversationMessage> AddMessageAsync(Guid conversationId, MessageRole role, string content, 
        Guid? sourceFAQId = null, double? confidenceScore = null, CancellationToken cancellationToken = default)
    {
        var message = new ConversationMessage
        {
            ConversationId = conversationId,
            Role = role,
            Content = content,
            SourceFAQId = sourceFAQId,
            ConfidenceScore = confidenceScore
        };
        
        var savedMessage = await _repository.AddMessageAsync(message, cancellationToken);
        
        // Update conversation title from first user message if not set
        var conversation = await _repository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation != null && string.IsNullOrEmpty(conversation.Title) && role == MessageRole.User)
        {
            conversation.Title = content.Length > 50 ? content[..50] + "..." : content;
            await _repository.UpdateAsync(conversation, cancellationToken);
        }
        
        return savedMessage;
    }

    public async Task MarkMessageHelpfulAsync(Guid messageId, bool isHelpful, CancellationToken cancellationToken = default)
    {
        var message = await _repository.GetMessageByIdAsync(messageId, cancellationToken);
        if (message != null)
        {
            message.IsHelpful = isHelpful;
            await _repository.UpdateMessageAsync(message, cancellationToken);
        }
    }

    public async Task<bool> HandoffToSupportAsync(Guid conversationId, string reason, Guid? supportAgentId = null, 
        CancellationToken cancellationToken = default)
    {
        var conversation = await _repository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation != null)
        {
            conversation.HandedOffToSupport = true;
            conversation.HandoffReason = reason;
            conversation.SupportAgentId = supportAgentId;
            await _repository.UpdateAsync(conversation, cancellationToken);
            
            // Add system message about handoff
            await AddMessageAsync(conversationId, MessageRole.System, 
                $"Conversation handed off to human support. Reason: {reason}", 
                cancellationToken: cancellationToken);
            
            return true;
        }
        return false;
    }

    public async Task<bool> CloseConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _repository.GetByIdAsync(conversationId, cancellationToken);
        if (conversation != null)
        {
            conversation.IsClosed = true;
            conversation.ClosedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(conversation, cancellationToken);
            return true;
        }
        return false;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }
}
