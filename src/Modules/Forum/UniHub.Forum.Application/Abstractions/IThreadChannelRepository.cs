using UniHub.Forum.Domain.ThreadChannels;

namespace UniHub.Forum.Application.Abstractions;

public interface IThreadChannelRepository
{
    Task<IReadOnlyList<ThreadChannel>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken = default);
    Task<ThreadChannel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ThreadChannel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> ExistsByCodeAsync(string code, Guid? excludingId = null, CancellationToken cancellationToken = default);
    Task AddAsync(ThreadChannel channel, CancellationToken cancellationToken = default);
    Task UpdateAsync(ThreadChannel channel, CancellationToken cancellationToken = default);
}
