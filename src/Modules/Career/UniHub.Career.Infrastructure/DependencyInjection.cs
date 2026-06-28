using Microsoft.Extensions.DependencyInjection;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Infrastructure.Persistence.Repositories;

namespace UniHub.Career.Infrastructure;

/// <summary>
/// Dependency injection configuration for Career Infrastructure layer.
/// Registers all Career module services including repositories.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Career Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddCareerInfrastructure(this IServiceCollection services)
    {
        services.AddRepositories();

        return services;
    }

    /// <summary>
    /// Registers all Career repository implementations with EF Core.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IJobPostingRepository, JobPostingRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<IRecruiterRepository, RecruiterRepository>();
        services.AddScoped<ISavedJobRepository, SavedJobRepository>();

        return services;
    }
}
