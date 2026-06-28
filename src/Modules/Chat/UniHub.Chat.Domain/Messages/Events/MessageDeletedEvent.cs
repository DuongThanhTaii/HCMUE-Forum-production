using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Messages.Events;

/// <summary>
/// Domain event khi message bị xóa (soft delete)
/// </summary>
public sealed record MessageDeletedEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid DeletedBy,
    DateTime DeletedAt) : IDomainEvent;
