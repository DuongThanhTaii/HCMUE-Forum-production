using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application;

internal static class ChatSafetyHelper
{
    public static Guid? GetDirectPeerId(Conversation conversation, Guid userId)
    {
        if (conversation.Type != ConversationType.Direct)
        {
            return null;
        }

        var peer = conversation.Participants.FirstOrDefault(p => p != userId);
        return peer == Guid.Empty ? null : peer;
    }

    public static async Task<Result?> EnsureNotBlockedForDirectAsync(
        Conversation conversation,
        Guid senderId,
        IUserBlockChecker blockChecker,
        CancellationToken cancellationToken)
    {
        var peerId = GetDirectPeerId(conversation, senderId);
        if (peerId is null)
        {
            return null;
        }

        if (await blockChecker.IsBlockedEitherWayAsync(senderId, peerId.Value, cancellationToken))
        {
            return Result.Failure(new Error(
                "Chat.UserBlocked",
                "You cannot message this user because one of you has blocked the other"));
        }

        return null;
    }
}
