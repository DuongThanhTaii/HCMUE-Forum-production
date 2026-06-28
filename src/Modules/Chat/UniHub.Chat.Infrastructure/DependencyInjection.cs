using Microsoft.Extensions.DependencyInjection;
using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Infrastructure.Persistence.Repositories;
using UniHub.Chat.Infrastructure.Services;

namespace UniHub.Chat.Infrastructure;

/// <summary>
/// Dependency injection configuration for Chat Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds Chat Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="uploadPath">Path for file uploads (defaults to wwwroot/uploads/chat)</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddChatInfrastructure(
        this IServiceCollection services,
        string? uploadPath = null)
    {
        services.AddRepositories();
        services.AddServices(uploadPath);
        services.AddScoped<IConversationParticipantLookup, ConversationParticipantLookup>();

        return services;
    }

    /// <summary>
    /// Registers all Chat repository implementations.
    /// </summary>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IConversationMuteRepository, ConversationMuteRepository>();
        services.AddScoped<IChatMessageReportRepository, ChatMessageReportRepository>();

        return services;
    }

    /// <summary>
    /// Registers all Chat services.
    /// </summary>
    private static IServiceCollection AddServices(
        this IServiceCollection services,
        string? uploadPath)
    {
        var finalUploadPath = uploadPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "chat");

        services.AddScoped<IFileStorageService>(_ =>
            new LocalFileStorageService(finalUploadPath));
        services.AddScoped<IUserBlockChecker, UserBlockChecker>();

        return services;
    }
}
