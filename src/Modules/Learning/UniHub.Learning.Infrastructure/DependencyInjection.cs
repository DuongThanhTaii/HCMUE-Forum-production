using Microsoft.Extensions.DependencyInjection;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Infrastructure.Persistence.Repositories;
using UniHub.Learning.Infrastructure.Services;

namespace UniHub.Learning.Infrastructure;

/// <summary>
/// Dependency injection configuration for Learning Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Learning Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddLearningInfrastructure(this IServiceCollection services)
    {
        services.AddRepositories();
        services.AddServices();

        return services;
    }

    /// <summary>
    /// Registers all Learning repository implementations with EF Core.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();

        return services;
    }

    /// <summary>
    /// Registers all Learning service implementations.
    /// </summary>
    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        // Bind Cloudinary settings
        services.AddOptions<CloudinarySettings>().BindConfiguration("Cloudinary");

        // File storage - Cloudinary implementation
        services.AddScoped<IFileStorageService, FileStorageService>();
        
        // Virus scanning - stub implementation
        services.AddScoped<IVirusScanService, VirusScanService>();
        
        // User tracking services
        services.AddScoped<IUserRatingService, UserRatingService>();
        services.AddScoped<IUserDownloadService, UserDownloadService>();
        services.AddScoped<IDocumentDetailEnricher, DocumentDetailEnricher>();
        
        // Permission services
        services.AddScoped<IModeratorPermissionService, ModeratorPermissionService>();
        services.AddScoped<IModeratorManagementPermissionService, ModeratorManagementPermissionService>();

        return services;
    }
}
