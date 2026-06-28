using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Conversations.Events;

/// <summary>
/// Event khi conversation được unarchive
/// </summary>
public sealed record ConversationUnarchivedEvent(
    Guid ConversationId,
    Guid UnarchivedBy,
    DateTime UnarchivedAt) : IDomainEvent;
