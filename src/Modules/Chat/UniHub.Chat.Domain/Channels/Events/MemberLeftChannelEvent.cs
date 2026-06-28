using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Channels.Events;

/// <summary>
/// Domain event khi member leave channel
/// </summary>
public sealed record MemberLeftChannelEvent(
    Guid ChannelId,
    Guid MemberId,
    DateTime LeftAt) : IDomainEvent;
