using UniHub.Chat.Domain.Channels;

namespace UniHub.Chat.Application.Abstractions;

/// <summary>
/// Repository interface for Channel aggregate
/// </summary>
public interface IChannelRepository
{
    /// <summary>
    /// Get channel by ID
    /// </summary>
    Task<Channel?> GetByIdAsync(ChannelId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all public channels (for discovery)
    /// </summary>
    Task<IReadOnlyList<Channel>> GetPublicChannelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get channels where user is a member
    /// </summary>
    Task<IReadOnlyList<Channel>> GetByMemberIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if channel exists
    /// </summary>
    Task<bool> ExistsAsync(ChannelId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add new channel
    /// </summary>
    Task AddAsync(Channel channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update existing channel
    /// </summary>
    Task UpdateAsync(Channel channel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete channel (for archive functionality)
    /// </summary>
    Task DeleteAsync(Channel channel, CancellationToken cancellationToken = default);
}
