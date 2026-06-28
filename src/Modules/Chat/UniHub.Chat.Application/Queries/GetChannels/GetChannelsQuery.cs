using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Queries.GetChannels;

/// <summary>
/// Response for a channel in the list
/// </summary>
public sealed record ChannelResponse(
    Guid Id,
    string Name,
    string? Description,
    string Type,
    Guid OwnerId,
    int MemberCount,
    DateTime CreatedAt,
    bool IsArchived);

/// <summary>
/// Query to get channels (public for discovery, or user's channels)
/// </summary>
public sealed record GetChannelsQuery(
    Guid? UserId = null, 
    bool? PublicOnly = null) : IQuery<IReadOnlyList<ChannelResponse>>;
