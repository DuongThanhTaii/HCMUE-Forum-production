using UniHub.Chat.Application.Queries.GetMessages;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Queries.ListConversationAttachments;

public sealed record ConversationAttachmentResponse(
    Guid MessageId,
    DateTime SentAt,
    string FileName,
    string FileUrl,
    string MimeType,
    string? ThumbnailUrl,
    long FileSize);

public sealed record ListConversationAttachmentsQuery(
    Guid UserId,
    Guid ConversationId,
    string Kind = "all",
    int Page = 1,
    int PageSize = 30) : IQuery<PagedResponse<ConversationAttachmentResponse>>;
