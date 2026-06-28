using UniHub.Chat.Application.Queries.GetMessages;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Queries.ListConversationLinks;

public sealed class ListConversationLinksQueryHandler
    : IQueryHandler<ListConversationLinksQuery, PagedResponse<ConversationLinkResponse>>
{
    private readonly IConversationRepository _conversationRepository;
    private readonly IMessageRepository _messageRepository;

    public ListConversationLinksQueryHandler(
        IConversationRepository conversationRepository,
        IMessageRepository messageRepository)
    {
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
    }

    public async Task<Result<PagedResponse<ConversationLinkResponse>>> Handle(
        ListConversationLinksQuery request,
        CancellationToken cancellationToken)
    {
        var conversationId = ConversationId.Create(request.ConversationId);

        if (!await _conversationRepository.ExistsAsync(conversationId, cancellationToken))
        {
            return Result.Failure<PagedResponse<ConversationLinkResponse>>(new Error(
                "Conversation.NotFound",
                $"Conversation with ID {request.ConversationId} not found"));
        }

        if (!await _conversationRepository.IsUserParticipantAsync(
                conversationId,
                request.UserId,
                cancellationToken))
        {
            return Result.Failure<PagedResponse<ConversationLinkResponse>>(new Error(
                "Conversation.NotParticipant",
                "User is not a participant in this conversation"));
        }

        var (messages, totalCount) = await _messageRepository.ListMessagesWithLinksAsync(
            conversationId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var links = new List<ConversationLinkResponse>();
        foreach (var message in messages)
        {
            foreach (var url in ConversationLinkExtractor.ExtractUrls(message.Content))
            {
                string host;
                try
                {
                    host = new Uri(url).Host;
                }
                catch
                {
                    host = url;
                }

                links.Add(new ConversationLinkResponse(
                    message.Id.Value,
                    message.SentAt,
                    url,
                    host));
            }
        }

        var totalPages = totalCount == 0
            ? 0
            : (int)Math.Ceiling((double)totalCount / request.PageSize);

        return Result.Success(new PagedResponse<ConversationLinkResponse>(
            links,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages));
    }
}
