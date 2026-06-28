using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of summary cache repository
/// </summary>
public class InMemorySummaryCacheRepository : ISummaryCacheRepository
{
    private readonly Dictionary<string, SummaryCacheEntry> _cache = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<SummaryCacheEntry?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out var entry))
            {
                // Check if expired
                if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < DateTime.UtcNow)
                {
                    _cache.Remove(cacheKey);
                    return null;
                }
                
                // Update access stats
                entry.AccessCount++;
                entry.LastAccessedAt = DateTime.UtcNow;
                
                return entry;
            }
            
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<SummaryCacheEntry> AddAsync(SummaryCacheEntry entry, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            _cache[entry.CacheKey] = entry;
            return entry;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> ExistsAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(cacheKey, out var entry))
            {
                // Check if expired
                if (entry.ExpiresAt.HasValue && entry.ExpiresAt.Value < DateTime.UtcNow)
                {
                    _cache.Remove(cacheKey);
                    return false;
                }
                return true;
            }
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _cache.Remove(cacheKey);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            _cache.Clear();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.ExpiresAt.HasValue && kvp.Value.ExpiresAt.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();
            
            foreach (var key in expiredKeys)
            {
                _cache.Remove(key);
            }
        }
        finally
        {
            _lock.Release();
        }
    }
}
