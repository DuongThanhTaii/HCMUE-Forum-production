using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Messages.Events;

/// <summary>
/// Domain event khi có reaction được thêm vào message
/// </summary>
public sealed record MessageReactionAddedEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid UserId,
    string Emoji,
    DateTime AddedAt) : IDomainEvent;
