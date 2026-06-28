using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Infrastructure;
using UniHub.Identity.Infrastructure.Caching;

namespace UniHub.Identity.Infrastructure.Tests.Caching;

public sealed class PermissionCacheRegistrationTests
{
    [Fact]
    public void AddIdentityInfrastructure_WhenProviderIsInMemory_ShouldRegisterInMemoryPermissionCache()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "ThisIsATestSecretKeyForPermissionCacheRegistrationTests_1234567890",
            ["Jwt:Issuer"] = "UniHub.Tests",
            ["Jwt:Audience"] = "UniHub.Tests.Client",
            ["Jwt:AccessTokenExpiryMinutes"] = "15",
            ["Jwt:RefreshTokenExpiryDays"] = "7",
            ["Identity:PermissionCache:Provider"] = "InMemory"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddIdentityInfrastructure(configuration);

        var descriptor = services.Last(item => item.ServiceType == typeof(IPermissionCache));

        descriptor.ImplementationType.Should().Be(typeof(InMemoryPermissionCache));
    }

    [Fact]
    public void AddIdentityInfrastructure_WhenProviderIsRedis_ShouldRegisterRedisPermissionCache()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "ThisIsATestSecretKeyForPermissionCacheRegistrationTests_1234567890",
            ["Jwt:Issuer"] = "UniHub.Tests",
            ["Jwt:Audience"] = "UniHub.Tests.Client",
            ["Jwt:AccessTokenExpiryMinutes"] = "15",
            ["Jwt:RefreshTokenExpiryDays"] = "7",
            ["Identity:PermissionCache:Provider"] = "Redis"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddIdentityInfrastructure(configuration);

        var descriptor = services.Last(item => item.ServiceType == typeof(IPermissionCache));

        descriptor.ImplementationType.Should().Be(typeof(RedisPermissionCache));
    }
}
