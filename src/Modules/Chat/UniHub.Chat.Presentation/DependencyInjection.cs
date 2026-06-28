using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniHub.Chat.Presentation.Options;
using UniHub.Chat.Presentation.Services;

namespace UniHub.Chat.Presentation;

/// <summary>
/// Dependency injection configuration for Chat Presentation layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Chat Presentation services to the service collection, including SignalR with optional Redis backplane.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration to read Redis backplane settings from.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddChatPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register Redis backplane options
        services.Configure<RedisBackplaneOptions>(
            configuration.GetSection(RedisBackplaneOptions.SectionName));

        var redisOptions = configuration
            .GetSection(RedisBackplaneOptions.SectionName)
            .Get<RedisBackplaneOptions>();

        // Determine if we're in development environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

        // Register SignalR
        var signalRBuilder = services.AddSignalR(options =>
        {
            // Enable detailed errors only in development
            options.EnableDetailedErrors = isDevelopment;
            
            // Configure keep-alive interval (default is 15 seconds)
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
            
            // Configure client timeout (default is 30 seconds)
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            
            // Maximum message size (default is 32KB)
            options.MaximumReceiveMessageSize = 128 * 1024; // 128KB
        });

        // Add Redis backplane if configured
        if (redisOptions?.ShouldUseRedis == true)
        {
            signalRBuilder.AddStackExchangeRedis(redisOptions.ConnectionString!, options =>
            {
                options.Configuration.ConnectTimeout = redisOptions.ConnectTimeoutMs;
                options.Configuration.SyncTimeout = redisOptions.SyncTimeoutMs;
                options.Configuration.AbortOnConnectFail = redisOptions.AbortOnConnectFail;
            });
        }

        // Register connection manager
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        return services;
    }
}
