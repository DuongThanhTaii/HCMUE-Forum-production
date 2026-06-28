using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Conversations.Events;

/// <summary>
/// Event khi conversation được archive
/// </summary>
public sealed record ConversationArchivedEvent(
    Guid ConversationId,
    Guid ArchivedBy,
    DateTime ArchivedAt) : IDomainEvent;
