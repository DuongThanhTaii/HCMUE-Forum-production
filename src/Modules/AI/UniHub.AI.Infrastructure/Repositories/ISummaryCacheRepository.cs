using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Repositories;

/// <summary>
/// Repository for summary cache operations
/// </summary>
public interface ISummaryCacheRepository
{
    Task<SummaryCacheEntry?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task<SummaryCacheEntry> AddAsync(SummaryCacheEntry entry, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task ClearAllAsync(CancellationToken cancellationToken = default);
    Task CleanupExpiredAsync(CancellationToken cancellationToken = default);
}
