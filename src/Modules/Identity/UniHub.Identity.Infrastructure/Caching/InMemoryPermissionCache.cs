using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Queries.GetUserPermissions;

namespace UniHub.Identity.Infrastructure.Caching;

/// <summary>
/// In-memory implementation of permission cache
/// </summary>
public sealed class InMemoryPermissionCache : IPermissionCache
{
    private readonly Dictionary<Guid, CacheEntry> _cache = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<UserPermissionsResponse?> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(userId, out var entry))
            {
                if (entry.ExpiresAt > DateTime.UtcNow)
                {
                    return entry.Permissions;
                }

                // Remove expired entry
                _cache.Remove(userId);
            }

            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SetUserPermissionsAsync(
        Guid userId,
        UserPermissionsResponse permissions,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var entry = new CacheEntry(permissions, DateTime.UtcNow.Add(_cacheExpiration));
            _cache[userId] = entry;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task InvalidateUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            _cache.Remove(userId);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
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

    private sealed record CacheEntry(UserPermissionsResponse Permissions, DateTime ExpiresAt);
}
