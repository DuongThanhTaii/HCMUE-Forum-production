using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of conversation repository
/// </summary>
public class InMemoryConversationRepository : IConversationRepository
{
    private readonly List<Conversation> _conversations = new();
    private readonly List<ConversationMessage> _messages = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var conversation = _conversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                conversation.Messages = _messages
                    .Where(m => m.ConversationId == id)
                    .OrderBy(m => m.SentAt)
                    .ToList();
            }
            return conversation;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<Conversation>> GetByUserIdAsync(Guid userId, int skip = 0, int take = 20, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var conversations = _conversations
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.LastActiveAt)
                .Skip(skip)
                .Take(take)
                .ToList();

            foreach (var conversation in conversations)
            {
                conversation.Messages = _messages
                    .Where(m => m.ConversationId == conversation.Id)
                    .OrderBy(m => m.SentAt)
                    .ToList();
            }

            return conversations;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Conversation?> GetBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var conversation = _conversations.FirstOrDefault(c => c.SessionId == sessionId && !c.IsClosed);
            if (conversation != null)
            {
                conversation.Messages = _messages
                    .Where(m => m.ConversationId == conversation.Id)
                    .OrderBy(m => m.SentAt)
                    .ToList();
            }
            return conversation;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Conversation> CreateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            conversation.Id = Guid.NewGuid();
            conversation.StartedAt = DateTime.UtcNow;
            conversation.LastActiveAt = DateTime.UtcNow;
            _conversations.Add(conversation);
            return conversation;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<Conversation> UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var existing = _conversations.FirstOrDefault(c => c.Id == conversation.Id);
            if (existing != null)
            {
                _conversations.Remove(existing);
                conversation.LastActiveAt = DateTime.UtcNow;
                _conversations.Add(conversation);
            }
            return conversation;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var conversation = _conversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                _conversations.Remove(conversation);
                _messages.RemoveAll(m => m.ConversationId == id);
                return true;
            }
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ConversationMessage> AddMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            message.Id = Guid.NewGuid();
            message.SentAt = DateTime.UtcNow;
            _messages.Add(message);
            
            // Update conversation last active time
            var conversation = _conversations.FirstOrDefault(c => c.Id == message.ConversationId);
            if (conversation != null)
            {
                conversation.LastActiveAt = DateTime.UtcNow;
            }
            
            return message;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ConversationMessage?> GetMessageByIdAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _messages.FirstOrDefault(m => m.Id == messageId);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<ConversationMessage> UpdateMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var existing = _messages.FirstOrDefault(m => m.Id == message.Id);
            if (existing != null)
            {
                _messages.Remove(existing);
                _messages.Add(message);
            }
            return message;
        }
        finally
        {
            _lock.Release();
        }
    }
}
