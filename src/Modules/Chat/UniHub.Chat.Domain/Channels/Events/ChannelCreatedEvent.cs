using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Channels.Events;

/// <summary>
/// Domain event khi channel được tạo
/// </summary>
public sealed record ChannelCreatedEvent(
    Guid ChannelId,
    string Name,
    string? Description,
    ChannelType Type,
    Guid CreatedBy,
    DateTime CreatedAt) : IDomainEvent;
