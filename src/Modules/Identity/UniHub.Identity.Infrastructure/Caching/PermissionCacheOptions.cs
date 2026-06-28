namespace UniHub.Identity.Infrastructure.Caching;

public sealed class PermissionCacheOptions
{
    public const string SectionName = "Identity:PermissionCache";

    public string Provider { get; set; } = "InMemory";
    public int ExpirationMinutes { get; set; } = 15;
    public string RedisInstanceName { get; set; } = "UniHub:Identity";
}
