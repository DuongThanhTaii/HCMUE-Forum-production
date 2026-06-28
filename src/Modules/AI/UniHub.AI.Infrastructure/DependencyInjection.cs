using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UniHub.AI.Application.Abstractions;
using UniHub.AI.Application.Services;
using UniHub.AI.Domain.AIProviders;
using UniHub.AI.Infrastructure.Configuration;
using UniHub.AI.Infrastructure.Providers;
using UniHub.AI.Infrastructure.Repositories;
using UniHub.AI.Infrastructure.Services;

namespace UniHub.AI.Infrastructure;

/// <summary>
/// Dependency injection configuration for AI Infrastructure layer.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds AI Infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddAIInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure AI provider settings
        services.Configure<AIProvidersSettings>(
            configuration.GetSection(AIProvidersSettings.SectionName));
        
        // Configure moderation settings
        services.Configure<ModerationSettings>(
            configuration.GetSection(ModerationSettings.SectionName));
        
        // Configure summarization settings
        services.Configure<SummarizationSettings>(
            configuration.GetSection(SummarizationSettings.SectionName));
        
        // Configure smart search settings
        services.Configure<SmartSearchSettings>(
            configuration.GetSection(SmartSearchSettings.SectionName));

        // Register HttpClient for AI providers
        services.AddHttpClient();

        // Register AI providers
        services.AddAIProviders();

        // Register provider factory
        services.AddSingleton<IAIProviderFactory, AIProviderFactory>();
        
        // Register repositories
        services.AddSingleton<IFAQRepository, InMemoryFAQRepository>();
        services.AddSingleton<IConversationRepository, InMemoryConversationRepository>();
        services.AddSingleton<ISummaryCacheRepository, InMemorySummaryCacheRepository>();
        
        // Register services
        services.AddScoped<IFAQService, FAQService>();
        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IUniBotService, UniBotService>();
        services.AddScoped<IContentModerationService, ContentModerationService>();
        services.AddScoped<IDocumentSummarizationService, DocumentSummarizationService>();
        services.AddScoped<ISmartSearchService, SmartSearchService>();

        return services;
    }

    /// <summary>
    /// Registers all AI provider implementations.
    /// </summary>
    private static IServiceCollection AddAIProviders(this IServiceCollection services)
    {
        // Register Groq provider
        services.AddSingleton<IAIProvider>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AIProvidersSettings>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var config = new AIProviderConfiguration
            {
                ProviderType = AIProviderType.Groq,
                ApiKey = settings.Groq.ApiKey,
                BaseUrl = settings.Groq.BaseUrl,
                ModelName = settings.Groq.ModelName,
                MaxRequestsPerMinute = settings.Groq.MaxRequestsPerMinute,
                MaxTokensPerRequest = settings.Groq.MaxTokensPerRequest,
                IsEnabled = IsProviderConfigured(settings.Groq),
                Priority = settings.Groq.Priority,
                TimeoutSeconds = settings.Groq.TimeoutSeconds
            };
            return new GroqProvider(config, httpClientFactory);
        });

        // Register Gemini provider
        services.AddSingleton<IAIProvider>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AIProvidersSettings>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var config = new AIProviderConfiguration
            {
                ProviderType = AIProviderType.Gemini,
                ApiKey = settings.Gemini.ApiKey,
                BaseUrl = settings.Gemini.BaseUrl,
                ModelName = settings.Gemini.ModelName,
                MaxRequestsPerMinute = settings.Gemini.MaxRequestsPerMinute,
                MaxTokensPerRequest = settings.Gemini.MaxTokensPerRequest,
                IsEnabled = IsProviderConfigured(settings.Gemini),
                Priority = settings.Gemini.Priority,
                TimeoutSeconds = settings.Gemini.TimeoutSeconds
            };
            return new GeminiProvider(config, httpClientFactory);
        });

        // Register OpenRouter provider
        services.AddSingleton<IAIProvider>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AIProvidersSettings>>().Value;
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var config = new AIProviderConfiguration
            {
                ProviderType = AIProviderType.OpenRouter,
                ApiKey = settings.OpenRouter.ApiKey,
                BaseUrl = settings.OpenRouter.BaseUrl,
                ModelName = settings.OpenRouter.ModelName,
                MaxRequestsPerMinute = settings.OpenRouter.MaxRequestsPerMinute,
                MaxTokensPerRequest = settings.OpenRouter.MaxTokensPerRequest,
                IsEnabled = IsProviderConfigured(settings.OpenRouter),
                Priority = settings.OpenRouter.Priority,
                TimeoutSeconds = settings.OpenRouter.TimeoutSeconds
            };
            return new OpenRouterProvider(config, httpClientFactory);
        });

        return services;
    }

    private static bool IsProviderConfigured(ProviderSettings settings)
    {
        if (!settings.IsEnabled)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey) ||
            string.IsNullOrWhiteSpace(settings.ModelName) ||
            string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            return false;
        }

        return Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out _);
    }
}
