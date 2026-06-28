using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Channels.Events;

/// <summary>
/// Domain event khi member join channel
/// </summary>
public sealed record MemberJoinedChannelEvent(
    Guid ChannelId,
    Guid MemberId,
    DateTime JoinedAt) : IDomainEvent;
