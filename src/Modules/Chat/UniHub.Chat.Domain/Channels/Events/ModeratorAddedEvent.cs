using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Channels.Events;

/// <summary>
/// Domain event khi moderator được thêm vào channel
/// </summary>
public sealed record ModeratorAddedEvent(
    Guid ChannelId,
    Guid ModeratorId,
    Guid AddedBy,
    DateTime AddedAt) : IDomainEvent;
