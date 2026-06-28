using System.Collections.Concurrent;

namespace UniHub.Chat.Presentation.Services;

/// <summary>
/// Thread-safe in-memory implementation of IConnectionManager.
/// For production, consider using Redis for distributed scenarios.
/// </summary>
public sealed class ConnectionManager : IConnectionManager
{
    // ConnectionId -> UserId mapping
    private readonly ConcurrentDictionary<string, Guid> _connections = new();

    // UserId -> List<ConnectionId> mapping (supports multiple devices)
    private readonly ConcurrentDictionary<Guid, HashSet<string>> _userConnections = new();

    // ConversationId -> List<UserId> mapping
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _conversationUsers = new();

    // ChannelId -> List<UserId> mapping
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _channelUsers = new();

    // UserId -> List<ConversationId> mapping
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _userConversations = new();

    // UserId -> List<ChannelId> mapping
    private readonly ConcurrentDictionary<Guid, HashSet<Guid>> _userChannels = new();

    public Task AddConnectionAsync(Guid userId, string connectionId)
    {
        _connections.TryAdd(connectionId, userId);

        _userConnections.AddOrUpdate(
            userId,
            _ => new HashSet<string> { connectionId },
            (_, connections) =>
            {
                lock (connections)
                {
                    connections.Add(connectionId);
                    return connections;
                }
            });

        return Task.CompletedTask;
    }

    public Task RemoveConnectionAsync(string connectionId)
    {
        if (!_connections.TryRemove(connectionId, out var userId))
        {
            return Task.CompletedTask;
        }

        // Remove connection from user's connection list
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }
        }

        // Clean up user from all conversations and channels if no more connections
        if (!_userConnections.ContainsKey(userId))
        {
            CleanupUserPresence(userId);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetUserConnectionsAsync(Guid userId)
    {
        if (_userConnections.TryGetValue(userId, out var connections))
        {
            lock (connections)
            {
                return Task.FromResult<IReadOnlyList<string>>(connections.ToList());
            }
        }

        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    public Task<Guid?> GetUserIdAsync(string connectionId)
    {
        _connections.TryGetValue(connectionId, out var userId);
        return Task.FromResult<Guid?>(userId == Guid.Empty ? null : userId);
    }

    public Task JoinConversationAsync(string connectionId, Guid conversationId)
    {
        if (!_connections.TryGetValue(connectionId, out var userId))
        {
            return Task.CompletedTask;
        }

        // Add user to conversation
        _conversationUsers.AddOrUpdate(
            conversationId,
            _ => new HashSet<Guid> { userId },
            (_, users) =>
            {
                lock (users)
                {
                    users.Add(userId);
                    return users;
                }
            });

        // Add conversation to user's conversations
        _userConversations.AddOrUpdate(
            userId,
            _ => new HashSet<Guid> { conversationId },
            (_, conversations) =>
            {
                lock (conversations)
                {
                    conversations.Add(conversationId);
                    return conversations;
                }
            });

        return Task.CompletedTask;
    }

    public Task LeaveConversationAsync(string connectionId, Guid conversationId)
    {
        if (!_connections.TryGetValue(connectionId, out var userId))
        {
            return Task.CompletedTask;
        }

        // Remove user from conversation
        if (_conversationUsers.TryGetValue(conversationId, out var users))
        {
            lock (users)
            {
                users.Remove(userId);
                if (users.Count == 0)
                {
                    _conversationUsers.TryRemove(conversationId, out _);
                }
            }
        }

        // Remove conversation from user's conversations
        if (_userConversations.TryGetValue(userId, out var conversations))
        {
            lock (conversations)
            {
                conversations.Remove(conversationId);
                if (conversations.Count == 0)
                {
                    _userConversations.TryRemove(userId, out _);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task JoinChannelAsync(string connectionId, Guid channelId)
    {
        if (!_connections.TryGetValue(connectionId, out var userId))
        {
            return Task.CompletedTask;
        }

        // Add user to channel
        _channelUsers.AddOrUpdate(
            channelId,
            _ => new HashSet<Guid> { userId },
            (_, users) =>
            {
                lock (users)
                {
                    users.Add(userId);
                    return users;
                }
            });

        // Add channel to user's channels
        _userChannels.AddOrUpdate(
            userId,
            _ => new HashSet<Guid> { channelId },
            (_, channels) =>
            {
                lock (channels)
                {
                    channels.Add(channelId);
                    return channels;
                }
            });

        return Task.CompletedTask;
    }

    public Task LeaveChannelAsync(string connectionId, Guid channelId)
    {
        if (!_connections.TryGetValue(connectionId, out var userId))
        {
            return Task.CompletedTask;
        }

        // Remove user from channel
        if (_channelUsers.TryGetValue(channelId, out var users))
        {
            lock (users)
            {
                users.Remove(userId);
                if (users.Count == 0)
                {
                    _channelUsers.TryRemove(channelId, out _);
                }
            }
        }

        // Remove channel from user's channels
        if (_userChannels.TryGetValue(userId, out var channels))
        {
            lock (channels)
            {
                channels.Remove(channelId);
                if (channels.Count == 0)
                {
                    _userChannels.TryRemove(userId, out _);
                }
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Guid>> GetConversationUsersAsync(Guid conversationId)
    {
        if (_conversationUsers.TryGetValue(conversationId, out var users))
        {
            lock (users)
            {
                return Task.FromResult<IReadOnlyList<Guid>>(users.ToList());
            }
        }

        return Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());
    }

    public Task<IReadOnlyList<Guid>> GetChannelUsersAsync(Guid channelId)
    {
        if (_channelUsers.TryGetValue(channelId, out var users))
        {
            lock (users)
            {
                return Task.FromResult<IReadOnlyList<Guid>>(users.ToList());
            }
        }

        return Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());
    }

    public Task<IReadOnlyList<Guid>> GetUserConversationsAsync(Guid userId)
    {
        if (_userConversations.TryGetValue(userId, out var conversations))
        {
            lock (conversations)
            {
                return Task.FromResult<IReadOnlyList<Guid>>(conversations.ToList());
            }
        }

        return Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());
    }

    public Task<IReadOnlyList<Guid>> GetUserChannelsAsync(Guid userId)
    {
        if (_userChannels.TryGetValue(userId, out var channels))
        {
            lock (channels)
            {
                return Task.FromResult<IReadOnlyList<Guid>>(channels.ToList());
            }
        }

        return Task.FromResult<IReadOnlyList<Guid>>(Array.Empty<Guid>());
    }

    public Task<bool> IsUserOnlineAsync(Guid userId)
    {
        return Task.FromResult(_userConnections.ContainsKey(userId));
    }

    private void CleanupUserPresence(Guid userId)
    {
        // Remove user from all conversations
        if (_userConversations.TryRemove(userId, out var conversations))
        {
            foreach (var conversationId in conversations)
            {
                if (_conversationUsers.TryGetValue(conversationId, out var users))
                {
                    lock (users)
                    {
                        users.Remove(userId);
                        if (users.Count == 0)
                        {
                            _conversationUsers.TryRemove(conversationId, out _);
                        }
                    }
                }
            }
        }

        // Remove user from all channels
        if (_userChannels.TryRemove(userId, out var channels))
        {
            foreach (var channelId in channels)
            {
                if (_channelUsers.TryGetValue(channelId, out var users))
                {
                    lock (users)
                    {
                        users.Remove(userId);
                        if (users.Count == 0)
                        {
                            _channelUsers.TryRemove(channelId, out _);
                        }
                    }
                }
            }
        }
    }
}
