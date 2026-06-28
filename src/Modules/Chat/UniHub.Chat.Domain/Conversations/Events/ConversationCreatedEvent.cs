using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Conversations.Events;

/// <summary>
/// Event khi conversation được tạo
/// </summary>
public sealed record ConversationCreatedEvent(
    Guid ConversationId,
    ConversationType Type,
    IReadOnlyList<Guid> ParticipantIds,
    string? Title,
    Guid CreatedBy,
    DateTime CreatedAt) : IDomainEvent;
