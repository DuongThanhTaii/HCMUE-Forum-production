namespace UniHub.Chat.Presentation.Services;

/// <summary>
/// Manages SignalR connections and tracks which users are in which conversations and channels
/// </summary>
public interface IConnectionManager
{
    /// <summary>
    /// Register a connection for a user
    /// </summary>
    Task AddConnectionAsync(Guid userId, string connectionId);

    /// <summary>
    /// Remove a connection when user disconnects
    /// </summary>
    Task RemoveConnectionAsync(string connectionId);

    /// <summary>
    /// Get all connection IDs for a specific user (supports multiple devices)
    /// </summary>
    Task<IReadOnlyList<string>> GetUserConnectionsAsync(Guid userId);

    /// <summary>
    /// Get user ID for a specific connection
    /// </summary>
    Task<Guid?> GetUserIdAsync(string connectionId);

    /// <summary>
    /// Join a conversation group (for direct or group conversations)
    /// </summary>
    Task JoinConversationAsync(string connectionId, Guid conversationId);

    /// <summary>
    /// Leave a conversation group
    /// </summary>
    Task LeaveConversationAsync(string connectionId, Guid conversationId);

    /// <summary>
    /// Join a channel group
    /// </summary>
    Task JoinChannelAsync(string connectionId, Guid channelId);

    /// <summary>
    /// Leave a channel group
    /// </summary>
    Task LeaveChannelAsync(string connectionId, Guid channelId);

    /// <summary>
    /// Get all users currently in a conversation
    /// </summary>
    Task<IReadOnlyList<Guid>> GetConversationUsersAsync(Guid conversationId);

    /// <summary>
    /// Get all users currently in a channel
    /// </summary>
    Task<IReadOnlyList<Guid>> GetChannelUsersAsync(Guid channelId);

    /// <summary>
    /// Get all conversations a user is currently connected to
    /// </summary>
    Task<IReadOnlyList<Guid>> GetUserConversationsAsync(Guid userId);

    /// <summary>
    /// Get all channels a user is currently connected to
    /// </summary>
    Task<IReadOnlyList<Guid>> GetUserChannelsAsync(Guid userId);

    /// <summary>
    /// Check if a user is online (has any active connections)
    /// </summary>
    Task<bool> IsUserOnlineAsync(Guid userId);
}
