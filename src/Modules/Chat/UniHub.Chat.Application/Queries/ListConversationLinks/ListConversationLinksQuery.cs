using UniHub.Chat.Application.Queries.GetMessages;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Queries.ListConversationLinks;

public sealed record ConversationLinkResponse(
    Guid MessageId,
    DateTime SentAt,
    string Url,
    string Host);

public sealed record ListConversationLinksQuery(
    Guid UserId,
    Guid ConversationId,
    int Page = 1,
    int PageSize = 30) : IQuery<PagedResponse<ConversationLinkResponse>>;
