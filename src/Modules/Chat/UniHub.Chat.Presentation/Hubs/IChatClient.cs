namespace UniHub.Chat.Presentation.Hubs;

/// <summary>
/// Strongly-typed interface for SignalR client methods.
/// Defines methods that can be invoked on connected clients.
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// Client receives a new message in a conversation or channel
    /// </summary>
    Task ReceiveMessage(MessageNotification message);

    /// <summary>
    /// Client receives notification that a message was edited
    /// </summary>
    Task MessageEdited(MessageEditedNotification notification);

    /// <summary>
    /// Client receives notification that a message was deleted
    /// </summary>
    Task MessageDeleted(MessageDeletedNotification notification);

    /// <summary>
    /// Client receives notification that a user joined a conversation or channel
    /// </summary>
    Task UserJoined(UserJoinedNotification notification);

    /// <summary>
    /// Client receives notification that a user left a conversation or channel
    /// </summary>
    Task UserLeft(UserLeftNotification notification);

    /// <summary>
    /// Client receives notification that a user is typing
    /// </summary>
    Task UserTyping(TypingNotification notification);

    /// <summary>
    /// Client receives notification that a reaction was added to a message
    /// </summary>
    Task ReactionAdded(ReactionNotification notification);

    /// <summary>
    /// Client receives notification that a reaction was removed from a message
    /// </summary>
    Task ReactionRemoved(ReactionNotification notification);

    /// <summary>
    /// Client receives notification that a message was read by a user
    /// </summary>
    Task MessageRead(MessageReadNotification notification);

    /// <summary>
    /// Client receives notification that a channel was updated
    /// </summary>
    Task ChannelUpdated(ChannelUpdateNotification notification);

    /// <summary>
    /// Client receives notification that a user's online status changed
    /// </summary>
    Task UserStatusChanged(UserStatusNotification notification);

    /// <summary>
    /// WebRTC signaling (offer, answer, ICE, hangup) between two participants in a conversation
    /// </summary>
    Task ReceiveWebRtcSignal(WebRtcSignalNotification notification);
}

/// <summary>
/// WebRTC signal relayed to a peer. <see cref="Kind"/> is lower-case: offer, answer, ice, hangup.
/// </summary>
public sealed record WebRtcSignalNotification(
    Guid ConversationId,
    Guid FromUserId,
    string FromUserName,
    string Kind,
    string Payload);

/// <summary>
/// Notification sent when a new message is received
/// </summary>
public sealed record MessageNotification(
    Guid MessageId,
    Guid ConversationId,
    Guid? ChannelId,
    Guid SenderId,
    string SenderName,
    string Content,
    string MessageType,
    DateTime SentAt,
    Guid? ReplyToMessageId = null);

/// <summary>
/// Notification sent when a message is edited
/// </summary>
public sealed record MessageEditedNotification(
    Guid MessageId,
    Guid ConversationId,
    string NewContent,
    DateTime EditedAt);

/// <summary>
/// Notification sent when a message is deleted
/// </summary>
public sealed record MessageDeletedNotification(
    Guid MessageId,
    Guid ConversationId,
    DateTime DeletedAt);

/// <summary>
/// Notification sent when a user joins a conversation or channel
/// </summary>
public sealed record UserJoinedNotification(
    Guid UserId,
    string UserName,
    Guid? ConversationId,
    Guid? ChannelId,
    DateTime JoinedAt);

/// <summary>
/// Notification sent when a user leaves a conversation or channel
/// </summary>
public sealed record UserLeftNotification(
    Guid UserId,
    string UserName,
    Guid? ConversationId,
    Guid? ChannelId,
    DateTime LeftAt);

/// <summary>
/// Notification sent when a user is typing
/// </summary>
public sealed record TypingNotification(
    Guid UserId,
    string UserName,
    Guid ConversationId,
    bool IsTyping);

/// <summary>
/// Notification sent when a reaction is added or removed
/// </summary>
public sealed record ReactionNotification(
    Guid MessageId,
    Guid ConversationId,
    Guid UserId,
    string UserName,
    string Emoji,
    DateTime Timestamp);

/// <summary>
/// Notification sent when a message is read
/// </summary>
public sealed record MessageReadNotification(
    Guid MessageId,
    Guid ConversationId,
    Guid UserId,
    DateTime ReadAt);

/// <summary>
/// Notification sent when a channel is updated
/// </summary>
public sealed record ChannelUpdateNotification(
    Guid ChannelId,
    string NewName,
    string? NewDescription,
    DateTime UpdatedAt);

/// <summary>
/// Notification sent when a user's online status changes
/// </summary>
public sealed record UserStatusNotification(
    Guid UserId,
    string UserName,
    string Status,
    DateTime Timestamp);
