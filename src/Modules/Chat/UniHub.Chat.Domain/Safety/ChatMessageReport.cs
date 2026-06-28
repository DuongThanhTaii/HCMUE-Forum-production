using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Domain.Safety;

public sealed class ChatMessageReport
{
    public int Id { get; private set; }
    public Guid MessageId { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid ReporterId { get; private set; }
    public ChatMessageReportReason Reason { get; private set; }
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ChatMessageReport() { }

    private ChatMessageReport(
        Guid messageId,
        Guid conversationId,
        Guid reporterId,
        ChatMessageReportReason reason,
        string? description,
        DateTime createdAt)
    {
        MessageId = messageId;
        ConversationId = conversationId;
        ReporterId = reporterId;
        Reason = reason;
        Description = description;
        CreatedAt = createdAt;
    }

    public static Result<ChatMessageReport> Create(
        Guid messageId,
        Guid conversationId,
        Guid reporterId,
        ChatMessageReportReason reason,
        string? description)
    {
        if (messageId == Guid.Empty)
        {
            return Result.Failure<ChatMessageReport>(new Error("ChatReport.InvalidMessage", "Message is required"));
        }

        if (conversationId == Guid.Empty)
        {
            return Result.Failure<ChatMessageReport>(new Error("ChatReport.InvalidConversation", "Conversation is required"));
        }

        if (reporterId == Guid.Empty)
        {
            return Result.Failure<ChatMessageReport>(new Error("ChatReport.InvalidReporter", "Reporter is required"));
        }

        if (description is { Length: > 2000 })
        {
            return Result.Failure<ChatMessageReport>(new Error("ChatReport.DescriptionTooLong", "Description is too long"));
        }

        return Result.Success(new ChatMessageReport(
            messageId,
            conversationId,
            reporterId,
            reason,
            string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            DateTime.UtcNow));
    }
}
