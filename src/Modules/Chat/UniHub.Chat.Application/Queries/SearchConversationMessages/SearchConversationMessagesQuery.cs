using UniHub.Chat.Application.Queries.GetMessages;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Queries.SearchConversationMessages;

public sealed record MessageSearchHitResponse(
    Guid MessageId,
    DateTime SentAt,
    string Snippet,
    Guid SenderId,
    string? SenderDisplayName);

public sealed record SearchConversationMessagesQuery(
    Guid UserId,
    Guid ConversationId,
    string Q,
    string Filter = "all",
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResponse<MessageSearchHitResponse>>;
