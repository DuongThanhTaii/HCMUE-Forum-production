using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Channels.Events;

/// <summary>
/// Domain event khi channel info được update (name, description)
/// </summary>
public sealed record ChannelUpdatedEvent(
    Guid ChannelId,
    string NewName,
    string? NewDescription,
    Guid UpdatedBy,
    DateTime UpdatedAt) : IDomainEvent;
