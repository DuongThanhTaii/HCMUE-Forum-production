using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Npgsql;
using StackExchange.Redis;
using UniHub.Infrastructure.Caching;
using UniHub.Infrastructure.MongoDb;
using UniHub.Infrastructure.Persistence;
using UniHub.Infrastructure.Persistence.Interceptors;
using UniHub.SharedKernel.Persistence;

namespace UniHub.Infrastructure;

/// <summary>
/// Extension methods for configuring infrastructure services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds infrastructure services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);

        return services;
    }

    /// <summary>
    /// Adds persistence services including DbContext and interceptors.
    /// </summary>
    private static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register interceptors
        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddScoped<DomainEventInterceptor>();

        // Register one shared NpgsqlDataSource for the whole app lifetime.
        // Building a new data source per DbContext instance can exhaust PostgreSQL clients.
        services.AddSingleton<NpgsqlDataSource>(serviceProvider =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            dataSourceBuilder.EnableDynamicJson();
            return dataSourceBuilder.Build();
        });

        // Register DbContext
        services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
        {
            var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();

            options.UseNpgsql(dataSource, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

            // Add interceptors
            var auditableInterceptor = serviceProvider.GetRequiredService<AuditableEntityInterceptor>();
            var domainEventInterceptor = serviceProvider.GetRequiredService<DomainEventInterceptor>();
            options.AddInterceptors(auditableInterceptor, domainEventInterceptor);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            // Enable detailed errors in development
            var detailedErrors = configuration.GetSection("DetailedErrors").Get<bool>();
            if (detailedErrors)
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        // Register DbContext base class for UnitOfWork
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Add health checks
        services.AddHealthChecks()
            .AddCheck<PostgreSqlHealthCheck>(
                name: "postgresql",
                tags: new[] { "database", "postgresql" });

        return services;
    }

    /// <summary>
    /// Adds MongoDB services.
    /// </summary>
    private static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure MongoDB conventions
        MongoDbConfiguration.Configure();

        // Register settings
        var mongoSettings = configuration.GetSection(MongoDbSettings.SectionName).Get<MongoDbSettings>()
            ?? throw new InvalidOperationException($"MongoDB settings section '{MongoDbSettings.SectionName}' not found.");
        mongoSettings.ConnectionString = mongoSettings.ConnectionString.Trim();
        mongoSettings.DatabaseName = mongoSettings.DatabaseName.Trim();

        services.AddSingleton(mongoSettings);

        // Register MongoDB client
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider.GetRequiredService<MongoDbSettings>();
            var mongoClientSettings = MongoClientSettings.FromConnectionString(settings.ConnectionString);

            mongoClientSettings.ConnectTimeout = TimeSpan.FromSeconds(settings.ConnectionTimeoutSeconds);
            mongoClientSettings.ServerSelectionTimeout = TimeSpan.FromSeconds(settings.ServerSelectionTimeoutSeconds);
            mongoClientSettings.MaxConnectionPoolSize = settings.MaxConnectionPoolSize;
            mongoClientSettings.MinConnectionPoolSize = settings.MinConnectionPoolSize;

            return new MongoClient(mongoClientSettings);
        });

        // Register MongoDB database
        services.AddSingleton<IMongoDatabase>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<IMongoClient>();
            var settings = serviceProvider.GetRequiredService<MongoDbSettings>();
            return client.GetDatabase(settings.DatabaseName);
        });

        // Register MongoDbContext
        services.AddSingleton<MongoDbContext>();

        // Note: MongoDB health check requires AspNetCore.HealthChecks.MongoDb package
        // and is configured with: .AddMongoDb(connectionString)
        // For now, basic connectivity can be verified through the IMongoClient registration

        return services;
    }

    /// <summary>
    /// Adds Redis caching services.
    /// </summary>
    private static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnectionString = configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string not found.");

        // Register Redis connection multiplexer
        services.AddSingleton<IConnectionMultiplexer>(serviceProvider =>
        {
            var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
            configurationOptions.AbortOnConnectFail = false;
            configurationOptions.ConnectTimeout = 5000;
            configurationOptions.SyncTimeout = 5000;

            return ConnectionMultiplexer.Connect(configurationOptions);
        });

        // Register distributed cache with Redis
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "UniHub:";
        });

        // Register cache service
        services.AddSingleton<ICacheService, RedisCacheService>();

        // SignalR (and optional Redis backplane) is registered in Chat presentation — avoid duplicate AddSignalR here.

        // Add health check
        services.AddHealthChecks()
            .AddRedis(redisConnectionString, name: "redis", tags: new[] { "cache", "redis" });

        return services;
    }
}
