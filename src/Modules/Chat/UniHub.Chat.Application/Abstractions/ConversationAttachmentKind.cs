namespace UniHub.Chat.Application.Abstractions;

public enum ConversationAttachmentKind
{
    All,
    Image,
    File,
    Voice,
}

public sealed record ConversationAttachmentListItem(
    Guid MessageId,
    DateTime SentAt,
    string FileName,
    string FileUrl,
    string MimeType,
    string? ThumbnailUrl,
    long FileSizeBytes);

public sealed record ConversationLinkListItem(
    Guid MessageId,
    DateTime SentAt,
    string Url);
