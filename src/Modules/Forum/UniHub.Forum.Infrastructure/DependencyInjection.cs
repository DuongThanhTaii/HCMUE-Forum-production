using Microsoft.Extensions.DependencyInjection;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Infrastructure.Persistence.Repositories;
using UniHub.Forum.Infrastructure.Services;

namespace UniHub.Forum.Infrastructure;

/// <summary>
/// Dependency injection configuration for Forum Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Forum Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddForumInfrastructure(this IServiceCollection services)
    {
        services.AddRepositories();
        
        return services;
    }

    /// <summary>
    /// Registers all Forum repository implementations.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IBookmarkRepository, BookmarkRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IThreadChannelRepository, ThreadChannelRepository>();
        services.AddScoped<IModerationScopeService, ModerationScopeService>();

        return services;
    }
}
