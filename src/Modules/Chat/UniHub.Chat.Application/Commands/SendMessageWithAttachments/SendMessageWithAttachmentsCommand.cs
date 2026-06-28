using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.SendMessageWithAttachments;

/// <summary>
/// Command to send a message with file attachments
/// </summary>
public sealed record SendMessageWithAttachmentsCommand(
    Guid ConversationId,
    Guid SenderId,
    string? Content,
    List<AttachmentDto> Attachments,
    Guid? ReplyToMessageId = null) : ICommand<Guid>;

/// <summary>
/// DTO for message attachment
/// </summary>
public sealed record AttachmentDto(
    string FileName,
    string FileUrl,
    long FileSize,
    string MimeType,
    string? ThumbnailUrl = null);
