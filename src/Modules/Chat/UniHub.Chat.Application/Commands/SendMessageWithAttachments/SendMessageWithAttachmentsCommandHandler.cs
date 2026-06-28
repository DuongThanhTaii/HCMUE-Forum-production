using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.SendMessageWithAttachments;

/// <summary>
/// Handler for sending messages with attachments
/// </summary>
public sealed class SendMessageWithAttachmentsCommandHandler : ICommandHandler<SendMessageWithAttachmentsCommand, Guid>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IUserBlockChecker _userBlockChecker;

    public SendMessageWithAttachmentsCommandHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IUserBlockChecker userBlockChecker)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _userBlockChecker = userBlockChecker;
    }

    public async Task<Result<Guid>> Handle(
        SendMessageWithAttachmentsCommand request,
        CancellationToken cancellationToken)
    {
        // Validate conversation exists
        var conversationId = ConversationId.Create(request.ConversationId);
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<Guid>(new Error(
                "Conversation.NotFound",
                $"Conversation with ID {request.ConversationId} not found"));
        }

        // Validate sender is participant
        if (!conversation.Participants.Contains(request.SenderId))
        {
            return Result.Failure<Guid>(new Error(
                "Message.SenderNotParticipant",
                "Sender must be a participant in the conversation"));
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

        // Validate conversation is not archived
        if (conversation.IsArchived)
        {
            return Result.Failure<Guid>(new Error(
                "Conversation.Archived",
                "Cannot send message to archived conversation"));
        }

        // Convert DTOs to domain value objects
        var attachments = request.Attachments
            .Select(a => Attachment.Create(
                a.FileName,
                a.FileUrl,
                a.FileSize,
                a.MimeType,
                a.ThumbnailUrl))
            .ToList();

        // Check if all attachments are valid
        var failedAttachment = attachments.FirstOrDefault(a => a.IsFailure);
        if (failedAttachment != null)
        {
            return Result.Failure<Guid>(failedAttachment.Error);
        }

        var validAttachments = attachments.Select(a => a.Value).ToList();

        // Determine message type based on first attachment MIME type
        var firstMimeType = request.Attachments.First().MimeType.ToLowerInvariant();
        var messageType = firstMimeType switch
        {
            var mime when mime.StartsWith("image/") => MessageType.Image,
            var mime when mime.StartsWith("video/") => MessageType.Video,
            _ => MessageType.File
        };

        // Convert ReplyToMessageId if provided
        MessageId? replyToMessageId = request.ReplyToMessageId.HasValue
            ? MessageId.Create(request.ReplyToMessageId.Value)
            : null;

        // Create message with attachments
        var messageResult = Message.CreateWithAttachments(
            conversationId,
            request.SenderId,
            request.Content ?? string.Empty,
            messageType,
            validAttachments,
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

        return Result.Success(message.Id.Value);
    }
}
