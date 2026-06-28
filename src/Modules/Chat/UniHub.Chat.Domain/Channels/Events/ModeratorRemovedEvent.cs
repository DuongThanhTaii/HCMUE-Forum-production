using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Channels.Events;

/// <summary>
/// Domain event khi moderator bị remove khỏi channel
/// </summary>
public sealed record ModeratorRemovedEvent(
    Guid ChannelId,
    Guid ModeratorId,
    Guid RemovedBy,
    DateTime RemovedAt) : IDomainEvent;
