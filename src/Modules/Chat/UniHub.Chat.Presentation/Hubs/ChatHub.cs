using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Application.Commands.ReportCallEnded;
using UniHub.Chat.Application.Commands.ReportMissedCall;
using UniHub.Chat.Domain.Conversations;

namespace UniHub.Chat.Presentation.Hubs;

/// <summary>
/// SignalR Hub for real-time chat functionality.
/// Handles connections, message sending, typing indicators, reactions, and presence.
/// </summary>
[Authorize]
public sealed class ChatHub : Hub<IChatClient>
{
    private readonly Services.IConnectionManager _connectionManager;
    private readonly IConversationRepository _conversationRepository;
    private readonly ISender _sender;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(
        Services.IConnectionManager connectionManager,
        IConversationRepository conversationRepository,
        ISender sender,
        ILogger<ChatHub> logger)
    {
        _connectionManager = connectionManager;
        _conversationRepository = conversationRepository;
        _sender = sender;
        _logger = logger;
    }

    /// <summary>
    /// Called when a client connects to the hub
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await _connectionManager.AddConnectionAsync(userId.Value, Context.ConnectionId);
            _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", 
                userId.Value, Context.ConnectionId);

            // Notify other users about online status
            await Clients.Others.UserStatusChanged(new UserStatusNotification(
                userId.Value,
                GetUserName(),
                "Online",
                DateTime.UtcNow));
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the hub
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        if (userId.HasValue)
        {
            await _connectionManager.RemoveConnectionAsync(Context.ConnectionId);
            _logger.LogInformation("User {UserId} disconnected with ConnectionId {ConnectionId}", 
                userId.Value, Context.ConnectionId);

            // Check if user is still online on other devices
            var isStillOnline = await _connectionManager.IsUserOnlineAsync(userId.Value);
            if (!isStillOnline)
            {
                // Notify other users about offline status
                await Clients.Others.UserStatusChanged(new UserStatusNotification(
                    userId.Value,
                    GetUserName(),
                    "Offline",
                    DateTime.UtcNow));
            }
        }

        if (exception != null)
        {
            _logger.LogError(exception, "Error during disconnect for ConnectionId {ConnectionId}", 
                Context.ConnectionId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    #region Conversation Methods

    /// <summary>
    /// Join a conversation to receive messages
    /// </summary>
    public async Task JoinConversation(Guid conversationId)
    {
        try
        {
            await _connectionManager.JoinConversationAsync(Context.ConnectionId, conversationId);
            await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroup(conversationId));

            var userId = GetUserId();
            if (userId.HasValue)
            {
                _logger.LogInformation("User {UserId} joined conversation {ConversationId}", 
                    userId.Value, conversationId);

                // Notify other users in the conversation
                await Clients.OthersInGroup(GetConversationGroup(conversationId))
                    .UserJoined(new UserJoinedNotification(
                        userId.Value,
                        GetUserName(),
                        conversationId,
                        null,
                        DateTime.UtcNow));

                // Sync online/offline snapshot for peers in this conversation so client can show
                // "Đang hoạt động" immediately (without waiting for next connect/disconnect event).
                var conversation = await _conversationRepository.GetByIdAsync(
                    ConversationId.Create(conversationId),
                    Context.ConnectionAborted);
                if (conversation is not null)
                {
                    foreach (var participantId in conversation.Participants.Where(id => id != userId.Value))
                    {
                        var isOnline = await _connectionManager.IsUserOnlineAsync(participantId);
                        await Clients.Caller.UserStatusChanged(new UserStatusNotification(
                            participantId,
                            string.Empty,
                            isOnline ? "Online" : "Offline",
                            DateTime.UtcNow));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining conversation {ConversationId}", conversationId);
            throw;
        }
    }

    /// <summary>
    /// Leave a conversation
    /// </summary>
    public async Task LeaveConversation(Guid conversationId)
    {
        try
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                // Notify others before leaving
                await Clients.OthersInGroup(GetConversationGroup(conversationId))
                    .UserLeft(new UserLeftNotification(
                        userId.Value,
                        GetUserName(),
                        conversationId,
                        null,
                        DateTime.UtcNow));

                _logger.LogInformation("User {UserId} left conversation {ConversationId}", 
                    userId.Value, conversationId);
            }

            await _connectionManager.LeaveConversationAsync(Context.ConnectionId, conversationId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetConversationGroup(conversationId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving conversation {ConversationId}", conversationId);
            throw;
        }
    }

    /// <summary>
    /// Send a message to a conversation
    /// </summary>
    public async Task SendMessage(Guid conversationId, string content, string messageType, Guid? replyToMessageId = null)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new HubException("User not authenticated");
            }

            // In production, this would call Application layer to create the message
            // For now, we'll just broadcast a notification
            var notification = new MessageNotification(
                Guid.NewGuid(), // This should come from the created message
                conversationId,
                null,
                userId.Value,
                GetUserName(),
                content,
                messageType,
                DateTime.UtcNow,
                replyToMessageId);

            await Clients.Group(GetConversationGroup(conversationId))
                .ReceiveMessage(notification);

            _logger.LogInformation("User {UserId} sent message to conversation {ConversationId}", 
                userId.Value, conversationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to conversation {ConversationId}", conversationId);
            throw;
        }
    }

    /// <summary>
    /// Notify that user is typing in a conversation
    /// </summary>
    public async Task SendTypingIndicator(Guid conversationId, bool isTyping)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return;
            }

            await Clients.OthersInGroup(GetConversationGroup(conversationId))
                .UserTyping(new TypingNotification(
                    userId.Value,
                    GetUserName(),
                    conversationId,
                    isTyping));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending typing indicator for conversation {ConversationId}", 
                conversationId);
        }
    }

    #endregion

    #region Channel Methods

    /// <summary>
    /// Join a channel to receive messages
    /// </summary>
    public async Task JoinChannel(Guid channelId)
    {
        try
        {
            await _connectionManager.JoinChannelAsync(Context.ConnectionId, channelId);
            await Groups.AddToGroupAsync(Context.ConnectionId, GetChannelGroup(channelId));

            var userId = GetUserId();
            if (userId.HasValue)
            {
                _logger.LogInformation("User {UserId} joined channel {ChannelId}", 
                    userId.Value, channelId);

                // Notify other users in the channel
                await Clients.OthersInGroup(GetChannelGroup(channelId))
                    .UserJoined(new UserJoinedNotification(
                        userId.Value,
                        GetUserName(),
                        null,
                        channelId,
                        DateTime.UtcNow));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining channel {ChannelId}", channelId);
            throw;
        }
    }

    /// <summary>
    /// Leave a channel
    /// </summary>
    public async Task LeaveChannel(Guid channelId)
    {
        try
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                // Notify others before leaving
                await Clients.OthersInGroup(GetChannelGroup(channelId))
                    .UserLeft(new UserLeftNotification(
                        userId.Value,
                        GetUserName(),
                        null,
                        channelId,
                        DateTime.UtcNow));

                _logger.LogInformation("User {UserId} left channel {ChannelId}", 
                    userId.Value, channelId);
            }

            await _connectionManager.LeaveChannelAsync(Context.ConnectionId, channelId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetChannelGroup(channelId));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving channel {ChannelId}", channelId);
            throw;
        }
    }

    /// <summary>
    /// Send a message to a channel
    /// </summary>
    public async Task SendChannelMessage(Guid channelId, string content, string messageType)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new HubException("User not authenticated");
            }

            // In production, this would call Application layer to create the message
            var notification = new MessageNotification(
                Guid.NewGuid(), // This should come from the created message
                Guid.Empty, // Channels don't have conversationId
                channelId,
                userId.Value,
                GetUserName(),
                content,
                messageType,
                DateTime.UtcNow);

            await Clients.Group(GetChannelGroup(channelId))
                .ReceiveMessage(notification);

            _logger.LogInformation("User {UserId} sent message to channel {ChannelId}", 
                userId.Value, channelId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to channel {ChannelId}", channelId);
            throw;
        }
    }

    #endregion

    #region Message Actions

    /// <summary>
    /// Add a reaction to a message
    /// </summary>
    public async Task AddReaction(Guid messageId, Guid conversationId, string emoji)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new HubException("User not authenticated");
            }

            var notification = new ReactionNotification(
                messageId,
                conversationId,
                userId.Value,
                GetUserName(),
                emoji,
                DateTime.UtcNow);

            await Clients.Group(GetConversationGroup(conversationId))
                .ReactionAdded(notification);

            _logger.LogInformation("User {UserId} added reaction {Emoji} to message {MessageId}", 
                userId.Value, emoji, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding reaction to message {MessageId}", messageId);
            throw;
        }
    }

    /// <summary>
    /// Remove a reaction from a message
    /// </summary>
    public async Task RemoveReaction(Guid messageId, Guid conversationId, string emoji)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new HubException("User not authenticated");
            }

            var notification = new ReactionNotification(
                messageId,
                conversationId,
                userId.Value,
                GetUserName(),
                emoji,
                DateTime.UtcNow);

            await Clients.Group(GetConversationGroup(conversationId))
                .ReactionRemoved(notification);

            _logger.LogInformation("User {UserId} removed reaction {Emoji} from message {MessageId}", 
                userId.Value, emoji, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing reaction from message {MessageId}", messageId);
            throw;
        }
    }

    /// <summary>
    /// Mark a message as read
    /// </summary>
    public async Task MarkMessageAsRead(Guid messageId, Guid conversationId)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return;
            }

            var notification = new MessageReadNotification(
                messageId,
                conversationId,
                userId.Value,
                DateTime.UtcNow);

            await Clients.OthersInGroup(GetConversationGroup(conversationId))
                .MessageRead(notification);

            _logger.LogDebug("User {UserId} marked message {MessageId} as read", 
                userId.Value, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking message {MessageId} as read", messageId);
        }
    }

    #endregion

    #region Missed call

    /// <summary>
    /// Called by the caller when they hang up before the callee answers.
    /// Persists a MissedCall message visible to both sides and broadcasts it via hub.
    /// </summary>
    public async Task ReportMissedCall(Guid conversationId)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new HubException("User not authenticated");
            }

            var command = new ReportMissedCallCommand(conversationId, userId.Value);
            var result = await _sender.Send(command, Context.ConnectionAborted);

            if (result.IsFailure)
            {
                _logger.LogWarning("ReportMissedCall failed: {Error}", result.Error.Message);
                return;
            }

            var notification = new MessageNotification(
                result.Value,
                conversationId,
                null,
                userId.Value,
                GetUserName(),
                string.Empty,
                "MissedCall",
                DateTime.UtcNow);

            await Clients.Group(GetConversationGroup(conversationId)).ReceiveMessage(notification);

            _logger.LogInformation("MissedCall recorded for conversation {ConversationId} by {UserId}",
                conversationId, userId.Value);
        }
        catch (Exception ex) when (ex is not HubException)
        {
            _logger.LogError(ex, "ReportMissedCall failed for conversation {ConversationId}", conversationId);
        }
    }

    #endregion

    #region Call ended

    /// <summary>
    /// Called by whoever hangs up a connected call. Persists a CallEnded message
    /// visible to both participants and broadcasts it via hub.
    /// </summary>
    public async Task ReportCallEnded(Guid conversationId, int? durationSeconds = null)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new HubException("User not authenticated");
            }

            int? sanitizedDuration = durationSeconds.HasValue && durationSeconds.Value > 0
                ? durationSeconds.Value
                : null;
            var command = new ReportCallEndedCommand(conversationId, userId.Value, sanitizedDuration);
            var result = await _sender.Send(command, Context.ConnectionAborted);

            if (result.IsFailure)
            {
                _logger.LogWarning("ReportCallEnded failed: {Error}", result.Error.Message);
                return;
            }

            var notification = new MessageNotification(
                result.Value,
                conversationId,
                null,
                userId.Value,
                GetUserName(),
                string.Empty,
                "CallEnded",
                DateTime.UtcNow);

            await Clients.Group(GetConversationGroup(conversationId)).ReceiveMessage(notification);

            _logger.LogInformation("CallEnded recorded for conversation {ConversationId} by {UserId}",
                conversationId, userId.Value);
        }
        catch (Exception ex) when (ex is not HubException)
        {
            _logger.LogError(ex, "ReportCallEnded failed for conversation {ConversationId}", conversationId);
        }
    }

    #endregion

    #region WebRTC signaling

    /// <summary>
    /// Relay WebRTC SDP / ICE between two users who share the conversation (same membership rules as REST).
    /// </summary>
    public async Task RelayWebRtcSignal(Guid conversationId, Guid targetUserId, string kind, string? payload)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                throw new HubException("User not authenticated");
            }

            if (string.IsNullOrWhiteSpace(kind))
            {
                throw new HubException("Invalid signal kind");
            }

            var k = kind.Trim().ToLowerInvariant();
            if (k is not ("offer" or "answer" or "ice" or "hangup"))
            {
                throw new HubException("Invalid signal kind");
            }

            payload ??= string.Empty;
            if (k != "hangup" && string.IsNullOrWhiteSpace(payload))
            {
                throw new HubException("Payload required");
            }

            if (userId.Value == targetUserId)
            {
                return;
            }

            var conversation = await _conversationRepository.GetByIdAsync(
                ConversationId.Create(conversationId),
                Context.ConnectionAborted);

            if (conversation is null)
            {
                _logger.LogWarning("RelayWebRtcSignal: conversation {ConversationId} not found", conversationId);
                return;
            }

            if (!conversation.Participants.Contains(userId.Value) ||
                !conversation.Participants.Contains(targetUserId))
            {
                _logger.LogWarning(
                    "RelayWebRtcSignal: sender or target not in conversation {ConversationId}",
                    conversationId);
                return;
            }

            var targetConnections = await _connectionManager.GetUserConnectionsAsync(targetUserId);
            if (targetConnections.Count == 0)
            {
                _logger.LogDebug("RelayWebRtcSignal: target user {TargetUserId} has no active connections", targetUserId);
                // Let the caller fail fast when establishing media (SDP); ICE/hangup stay best-effort silent.
                if (k is "offer" or "answer")
                {
                    throw new HubException("webrtc_peer_offline");
                }

                return;
            }

            var notification = new WebRtcSignalNotification(
                conversationId,
                userId.Value,
                GetUserName(),
                k,
                payload);

            await Clients.Clients(targetConnections).ReceiveWebRtcSignal(notification);

            _logger.LogDebug(
                "RelayWebRtcSignal: {Kind} from {FromUser} to {TargetUser} ({ConnCount} connections)",
                k,
                userId.Value,
                targetUserId,
                targetConnections.Count);
        }
        catch (Exception ex) when (ex is not HubException)
        {
            _logger.LogError(ex, "RelayWebRtcSignal failed for conversation {ConversationId}", conversationId);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    private Guid? GetUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private string GetUserName()
    {
        var user = Context.User;
        var claimValue = user?.FindFirst(ClaimTypes.Name)?.Value
            ?? user?.FindFirst("name")?.Value
            ?? user?.FindFirst("preferred_username")?.Value
            ?? user?.FindFirst(ClaimTypes.Email)?.Value
            ?? user?.FindFirst("email")?.Value
            ?? user?.FindFirst("sub")?.Value;

        if (!string.IsNullOrWhiteSpace(claimValue))
        {
            return claimValue.Trim();
        }

        var userId = GetUserId();
        return userId.HasValue ? $"User {userId.Value.ToString("N")[..8]}" : "User";
    }

    private static string GetConversationGroup(Guid conversationId)
    {
        return $"conversation:{conversationId}";
    }

    private static string GetChannelGroup(Guid channelId)
    {
        return $"channel:{channelId}";
    }

    #endregion
}
