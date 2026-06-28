using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Queries.GetMessages;

/// <summary>
/// File attachment in a message (for images, voice, documents).
/// </summary>
public sealed record AttachmentResponse(
    string FileName,
    string FileUrl,
    long FileSize,
    string MimeType,
    string? ThumbnailUrl);

/// <summary>
/// Response for a message
/// </summary>
public sealed record MessageResponse(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    string Type,
    DateTime SentAt,
    DateTime? EditedAt,
    bool IsDeleted,
    Guid? ReplyToMessageId,
    Dictionary<string, List<Guid>> Reactions,
    IReadOnlyList<AttachmentResponse> Attachments,
    string? SenderDisplayName);

/// <summary>
/// Query to get messages for a conversation
/// </summary>
public sealed record GetMessagesQuery(
    Guid ConversationId,
    int Page = 1,
    int PageSize = 50) : IQuery<PagedResponse<MessageResponse>>;

/// <summary>
/// Paged response wrapper
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
