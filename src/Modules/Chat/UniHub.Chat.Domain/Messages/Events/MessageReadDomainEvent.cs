using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Messages.Events;

/// <summary>
/// Domain event raised when a message is marked as read by a user
/// </summary>
public sealed record MessageReadDomainEvent(
    MessageId MessageId,
    Guid UserId,
    DateTime ReadAt) : IDomainEvent;
