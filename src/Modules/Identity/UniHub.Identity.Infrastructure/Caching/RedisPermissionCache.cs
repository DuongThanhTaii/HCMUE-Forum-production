using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Queries.GetUserPermissions;

namespace UniHub.Identity.Infrastructure.Caching;

public sealed class RedisPermissionCache : IPermissionCache
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IDistributedCache _distributedCache;
    private readonly IDatabase _redisDatabase;
    private readonly ILogger<RedisPermissionCache> _logger;
    private readonly string _instanceName;
    private readonly TimeSpan _expiration;

    public RedisPermissionCache(
        IDistributedCache distributedCache,
        IConnectionMultiplexer connectionMultiplexer,
        IOptions<PermissionCacheOptions> options,
        ILogger<RedisPermissionCache> logger)
    {
        _distributedCache = distributedCache;
        _redisDatabase = connectionMultiplexer.GetDatabase();
        _logger = logger;

        var settings = options.Value;
        _instanceName = string.IsNullOrWhiteSpace(settings.RedisInstanceName)
            ? "UniHub:Identity"
            : settings.RedisInstanceName.Trim();

        _expiration = settings.ExpirationMinutes > 0
            ? TimeSpan.FromMinutes(settings.ExpirationMinutes)
            : TimeSpan.FromMinutes(15);
    }

    public async Task<UserPermissionsResponse?> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheVersion = await GetCacheVersionAsync();
            var cacheKey = BuildUserCacheKey(userId, cacheVersion);

            var payload = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
            if (string.IsNullOrWhiteSpace(payload))
            {
                return null;
            }

            var permissions = JsonSerializer.Deserialize<UserPermissionsResponse>(payload, JsonOptions);
            if (permissions is null)
            {
                await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
                return null;
            }

            return permissions;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Permission cache read failed for user {UserId}. Falling back to source.", userId);
            return null;
        }
    }

    public async Task SetUserPermissionsAsync(
        Guid userId,
        UserPermissionsResponse permissions,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheVersion = await GetCacheVersionAsync();
            var cacheKey = BuildUserCacheKey(userId, cacheVersion);
            var payload = JsonSerializer.Serialize(permissions, JsonOptions);

            var cacheEntryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _expiration
            };

            await _distributedCache.SetStringAsync(cacheKey, payload, cacheEntryOptions, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Permission cache write failed for user {UserId}.", userId);
        }
    }

    public async Task InvalidateUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheVersion = await GetCacheVersionAsync();
            var cacheKey = BuildUserCacheKey(userId, cacheVersion);
            await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Permission cache user invalidation failed for user {UserId}.", userId);
        }
    }

    public async Task InvalidateAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _redisDatabase.StringIncrementAsync(BuildVersionKey());
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Permission cache full invalidation failed.");
        }
    }

    private async Task<long> GetCacheVersionAsync()
    {
        var versionKey = BuildVersionKey();
        var versionValue = await _redisDatabase.StringGetAsync(versionKey);

        if (!versionValue.HasValue)
        {
            await _redisDatabase.StringSetAsync(versionKey, 1, when: When.NotExists);
            return 1;
        }

        return long.TryParse(versionValue.ToString(), out var parsedVersion) && parsedVersion > 0
            ? parsedVersion
            : 1;
    }

    private string BuildVersionKey()
    {
        return $"{_instanceName}:PermissionCache:Version";
    }

    private string BuildUserCacheKey(Guid userId, long version)
    {
        return $"{_instanceName}:PermissionCache:v{version}:User:{userId:N}";
    }
}
