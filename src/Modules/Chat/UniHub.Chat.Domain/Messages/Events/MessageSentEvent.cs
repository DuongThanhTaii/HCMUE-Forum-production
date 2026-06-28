using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Messages.Events;

/// <summary>
/// Domain event khi message được gửi
/// </summary>
public sealed record MessageSentEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderId,
    MessageType Type,
    string Content,
    DateTime SentAt) : IDomainEvent;
