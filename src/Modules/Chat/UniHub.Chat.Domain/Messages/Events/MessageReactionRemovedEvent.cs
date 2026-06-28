using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Messages.Events;

/// <summary>
/// Domain event khi reaction bị remove khỏi message
/// </summary>
public sealed record MessageReactionRemovedEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid UserId,
    string Emoji,
    DateTime RemovedAt) : IDomainEvent;
