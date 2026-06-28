using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Channels.Events;

/// <summary>
/// Domain event khi channel bị archive
/// </summary>
public sealed record ChannelArchivedEvent(
    Guid ChannelId,
    Guid ArchivedBy,
    DateTime ArchivedAt) : IDomainEvent;
