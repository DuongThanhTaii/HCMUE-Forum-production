using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Conversations.Events;

/// <summary>
/// Event khi remove participant khỏi conversation
/// </summary>
public sealed record ParticipantRemovedEvent(
    Guid ConversationId,
    Guid ParticipantId,
    Guid RemovedBy,
    DateTime RemovedAt) : IDomainEvent;
