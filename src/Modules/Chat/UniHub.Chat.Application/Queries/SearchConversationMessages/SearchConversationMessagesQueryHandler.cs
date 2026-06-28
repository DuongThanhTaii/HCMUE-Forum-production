using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Application.Queries.GetMessages;
using UniHub.Chat.Domain.Conversations;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Queries.SearchConversationMessages;

public sealed class SearchConversationMessagesQueryHandler
    : IQueryHandler<SearchConversationMessagesQuery, PagedResponse<MessageSearchHitResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IConversationParticipantLookup _participantLookup;

    public SearchConversationMessagesQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository,
        IConversationParticipantLookup participantLookup)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _participantLookup = participantLookup;
    }

    public async Task<Result<PagedResponse<MessageSearchHitResponse>>> Handle(
        SearchConversationMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var conversationId = ConversationId.Create(request.ConversationId);
        var conversation = await _conversationRepository.GetByIdAsync(conversationId, cancellationToken);

        if (conversation is null)
        {
            return Result.Failure<PagedResponse<MessageSearchHitResponse>>(new Error(
                "Conversation.NotFound",
                $"Conversation with ID {request.ConversationId} not found"));
        }

        if (!conversation.Participants.Contains(request.UserId))
        {
            return Result.Failure<PagedResponse<MessageSearchHitResponse>>(new Error(
                "Conversation.NotParticipant",
                "User is not a participant in this conversation"));
        }

        var filter = ParseFilter(request.Filter);
        var (messages, totalCount) = await _messageRepository.SearchByConversationIdAsync(
            conversationId,
            request.Q.Trim(),
            filter,
            request.Page,
            request.PageSize,
            cancellationToken);

        var senderIds = messages.Select(m => m.SenderId).Distinct().ToList();
        var senderCards = await _participantLookup.GetByIdsAsync(senderIds, cancellationToken);

        var hits = messages
            .Select(m =>
            {
                string? displayName = null;
                if (senderCards.TryGetValue(m.SenderId, out var senderCard) &&
                    !string.IsNullOrWhiteSpace(senderCard.FullName))
                {
                    displayName = senderCard.FullName;
                }

                return new MessageSearchHitResponse(
                    m.Id.Value,
                    m.SentAt,
                    MessageSearchSnippet.Build(m.Content, request.Q.Trim()),
                    m.SenderId,
                    displayName);
            })
            .ToList();

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling((double)totalCount / request.PageSize);

        return Result.Success(new PagedResponse<MessageSearchHitResponse>(
            hits,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages));
    }

    private static ConversationMessageSearchFilter ParseFilter(string filter) =>
        filter.Trim().ToLowerInvariant() switch
        {
            "text" => ConversationMessageSearchFilter.Text,
            "media" => ConversationMessageSearchFilter.Media,
            "links" => ConversationMessageSearchFilter.Links,
            _ => ConversationMessageSearchFilter.All,
        };
}
