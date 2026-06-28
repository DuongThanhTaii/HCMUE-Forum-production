using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Conversations;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Queries.GetMessages;

/// <summary>
/// Handler for getting conversation messages
/// </summary>
public sealed class GetMessagesQueryHandler : IQueryHandler<GetMessagesQuery, PagedResponse<MessageResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationParticipantLookup _participantLookup;

    public GetMessagesQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IConversationParticipantLookup participantLookup)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _participantLookup = participantLookup;
    }

    public async Task<Result<PagedResponse<MessageResponse>>> Handle(
        GetMessagesQuery request,
        CancellationToken cancellationToken)
    {
        // Verify conversation exists
        var conversationId = ConversationId.Create(request.ConversationId);
        var exists = await _conversationRepository.ExistsAsync(conversationId, cancellationToken);

        if (!exists)
        {
            return Result.Failure<PagedResponse<MessageResponse>>(new Error(
                "Conversation.NotFound",
                $"Conversation with ID {request.ConversationId} not found"));
        }

        // Get messages
        var messages = await _messageRepository.GetByConversationIdAsync(
            conversationId,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Get total count for pagination
        var totalCount = await _messageRepository.CountByConversationIdAsync(conversationId, cancellationToken);

        var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
        var senderCards = await _participantLookup.GetByIdsAsync(senderIds, cancellationToken);

        // Map to response
        var messageResponses = messages
            .Select(m =>
            {
                string? displayName = null;
                if (senderCards.TryGetValue(m.SenderId, out var senderCard) &&
                    !string.IsNullOrWhiteSpace(senderCard.FullName))
                {
                    displayName = senderCard.FullName;
                }
                var masked = m.IsDeleted;
                var reactions = masked
                    ? new Dictionary<string, List<Guid>>()
                    : m.Reactions
                        .GroupBy(r => r.Emoji)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(r => r.UserId).ToList());
                var attachments = masked
                    ? new List<AttachmentResponse>()
                    : m.Attachments
                        .Select(a => new AttachmentResponse(
                            a.FileName,
                            a.FileUrl,
                            a.FileSizeBytes,
                            a.MimeType,
                            a.ThumbnailUrl))
                        .ToList();
                return new MessageResponse(
                    m.Id.Value,
                    m.ConversationId.Value,
                    m.SenderId,
                    masked ? string.Empty : m.Content,
                    m.Type.ToString(),
                    m.SentAt,
                    masked ? null : m.EditedAt,
                    m.IsDeleted,
                    m.ReplyToMessageId?.Value,
                    reactions,
                    attachments,
                    displayName);
            })
            .ToList();

        var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

        var response = new PagedResponse<MessageResponse>(
            messageResponses,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);

        return Result.Success(response);
    }
}
