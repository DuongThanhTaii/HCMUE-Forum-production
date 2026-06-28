using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace UniHub.Infrastructure.Caching;

/// <summary>
/// Redis implementation of the cache service.
/// </summary>
public sealed class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly DistributedCacheEntryOptions _defaultOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache.</param>
    /// <param name="connectionMultiplexer">The Redis connection multiplexer.</param>
    public RedisCacheService(IDistributedCache cache, IConnectionMultiplexer connectionMultiplexer)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _defaultOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
    }

    /// <inheritdoc/>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await _cache.GetAsync(key, cancellationToken);
        
        if (cachedValue is null || cachedValue.Length == 0)
        {
            return null;
        }

        var jsonString = System.Text.Encoding.UTF8.GetString(cachedValue);
        return JsonSerializer.Deserialize<T>(jsonString);
    }

    /// <inheritdoc/>
    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        var bytes = System.Text.Encoding.UTF8.GetBytes(serializedValue);
        
        var options = absoluteExpiration.HasValue
            ? new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = absoluteExpiration.Value }
            : _defaultOptions;

        await _cache.SetAsync(key, bytes, options, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var database = _connectionMultiplexer.GetDatabase();
        var endpoints = _connectionMultiplexer.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);
            var keys = server.Keys(pattern: pattern);

            foreach (var key in keys)
            {
                await database.KeyDeleteAsync(key);
            }
        }
    }

    /// <inheritdoc/>
    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? absoluteExpiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);

        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var value = await factory();

        if (value is not null)
        {
            await SetAsync(key, value, absoluteExpiration, cancellationToken);
        }

        return value;
    }
}
