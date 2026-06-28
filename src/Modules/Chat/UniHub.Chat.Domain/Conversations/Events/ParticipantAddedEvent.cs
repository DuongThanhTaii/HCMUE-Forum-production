using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Conversations.Events;

/// <summary>
/// Event khi thêm participant vào conversation
/// </summary>
public sealed record ParticipantAddedEvent(
    Guid ConversationId,
    Guid ParticipantId,
    Guid AddedBy,
    DateTime AddedAt) : IDomainEvent;
