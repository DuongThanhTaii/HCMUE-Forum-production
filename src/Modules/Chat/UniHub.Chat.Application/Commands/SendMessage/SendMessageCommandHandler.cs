using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.SendMessage;

/// <summary>
/// Handler for sending a text message
/// </summary>
public sealed class SendMessageCommandHandler : ICommandHandler<SendMessageCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserBlockChecker _userBlockChecker;

    public SendMessageCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUserBlockChecker userBlockChecker)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _userBlockChecker = userBlockChecker;
    }

    public async Task<Result<Guid>> Handle(
        SendMessageCommand request,
        CancellationToken cancellationToken)
    {
        // Get conversation
        var conversationId = ConversationId.Create(request.ConversationId);
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<Guid>(new Error(
                "Conversation.NotFound",
                $"Conversation with ID {request.ConversationId} not found"));
        }

        // Verify sender is a participant
        if (!conversation.Participants.Contains(request.SenderId))
        {
            return Result.Failure<Guid>(new Error(
                "Conversation.NotParticipant",
                "User is not a participant in this conversation"));
        }

        var blockFailure = await ChatSafetyHelper.EnsureNotBlockedForDirectAsync(
            conversation,
            request.SenderId,
            _userBlockChecker,
            cancellationToken);
        if (blockFailure is not null)
        {
            return Result.Failure<Guid>(blockFailure.Error);
        }

        // Handle reply-to if specified
        MessageId? replyToMessageId = null;
        if (request.ReplyToMessageId.HasValue)
        {
            replyToMessageId = MessageId.Create(request.ReplyToMessageId.Value);
            var replyToMessage = await _messageRepository.GetByIdAsync(replyToMessageId, cancellationToken);

            if (replyToMessage is null)
            {
                return Result.Failure<Guid>(new Error(
                    "Message.ReplyToNotFound",
                    $"Reply-to message with ID {request.ReplyToMessageId} not found"));
            }

            // Verify reply-to message is in same conversation
            if (replyToMessage.ConversationId.Value != request.ConversationId)
            {
                return Result.Failure<Guid>(new Error(
                    "Message.ReplyToWrongConversation",
                    "Reply-to message is not in the same conversation"));
            }
        }

        // Create message
        var messageResult = Message.CreateText(
            conversationId,
            request.SenderId,
            request.Content,
            replyToMessageId);

        if (messageResult.IsFailure)
        {
            return Result.Failure<Guid>(messageResult.Error);
        }

        var message = messageResult.Value;

        // Save message
        await _messageRepository.AddAsync(message, cancellationToken);

        // Update conversation last message time
        conversation.UpdateLastMessageTime(message.SentAt);
        await _conversationRepository.UpdateAsync(conversation, cancellationToken);

        // Note: Real-time notification will be sent via ChatHub after this command completes
        // The controller/hub will use the returned message ID to broadcast to clients

        return Result.Success(message.Id.Value);
    }
}
