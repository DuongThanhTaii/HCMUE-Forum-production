using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Messages.Events;

/// <summary>
/// Domain event khi message được edit
/// </summary>
public sealed record MessageEditedEvent(
    Guid MessageId,
    Guid ConversationId,
    string NewContent,
    DateTime EditedAt) : IDomainEvent;
