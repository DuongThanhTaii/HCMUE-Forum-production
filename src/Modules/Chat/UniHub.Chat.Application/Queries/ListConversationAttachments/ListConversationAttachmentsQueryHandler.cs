using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Application.Queries.GetMessages;
using UniHub.Chat.Domain.Conversations;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Queries.ListConversationAttachments;

public sealed class ListConversationAttachmentsQueryHandler
    : IQueryHandler<ListConversationAttachmentsQuery, PagedResponse<ConversationAttachmentResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;

    public ListConversationAttachmentsQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<Result<PagedResponse<ConversationAttachmentResponse>>> Handle(
        ListConversationAttachmentsQuery request,
        CancellationToken cancellationToken)
    {
        var conversationId = ConversationId.Create(request.ConversationId);

        if (!await _conversationRepository.ExistsAsync(conversationId, cancellationToken))
        {
            return Result.Failure<PagedResponse<ConversationAttachmentResponse>>(new Error(
                "Conversation.NotFound",
                $"Conversation with ID {request.ConversationId} not found"));
        }

        if (!await _conversationRepository.IsUserParticipantAsync(
                conversationId,
                request.UserId,
                cancellationToken))
        {
            return Result.Failure<PagedResponse<ConversationAttachmentResponse>>(new Error(
                "Conversation.NotParticipant",
                "User is not a participant in this conversation"));
        }

        var kind = ParseKind(request.Kind);
        var (items, totalCount) = await _messageRepository.ListAttachmentsByConversationIdAsync(
            conversationId,
            kind,
            request.Page,
            request.PageSize,
            cancellationToken);

        var responses = items
            .Select(i => new ConversationAttachmentResponse(
                i.MessageId,
                i.SentAt,
                i.FileName,
                i.FileUrl,
                i.MimeType,
                i.ThumbnailUrl,
                i.FileSizeBytes))
            .ToList();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling((double)totalCount / request.PageSize);

        return Result.Success(new PagedResponse<ConversationAttachmentResponse>(
            responses,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages));
    }

    private static ConversationAttachmentKind ParseKind(string kind) =>
        kind.Trim().ToLowerInvariant() switch
        {
            "image" => ConversationAttachmentKind.Image,
            "file" => ConversationAttachmentKind.File,
            "voice" => ConversationAttachmentKind.Voice,
            _ => ConversationAttachmentKind.All,
        };
}
