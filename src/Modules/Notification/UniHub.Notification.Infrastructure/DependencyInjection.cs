using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Abstractions.Notifications;
using UniHub.Notification.Application.Services;
using UniHub.Notification.Infrastructure.Persistence.Repositories;
using UniHub.Notification.Infrastructure.Services.Notifications;

namespace UniHub.Notification.Infrastructure;

/// <summary>
/// Dependency injection configuration for Notification Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Notification Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRepositories();
        services.AddNotificationServices(configuration);

        return services;
    }

    /// <summary>
    /// Registers all Notification repositories.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        return services;
    }

    /// <summary>
    /// Registers all Notification services.
    /// </summary>
    private static IServiceCollection AddNotificationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure Web Push settings
        services.Configure<WebPushSettings>(
            configuration.GetSection(WebPushSettings.SectionName));

        // Configure Email settings
        services.Configure<EmailSettings>(
            configuration.GetSection(EmailSettings.SectionName));

        // Register Push notification service
        services.AddScoped<IPushNotificationService, WebPushNotificationService>();

        // Register Email notification service
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        // Register In-App notification service
        services.AddScoped<IInAppNotificationService, InAppNotificationService>();

        services.AddScoped<InAppNotificationDispatcher>();

        return services;
    }
}
