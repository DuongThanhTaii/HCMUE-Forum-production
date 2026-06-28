namespace UniHub.AI.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for AI providers.
/// </summary>
public sealed class AIProvidersSettings
{
    public const string SectionName = "AIProviders";

    /// <summary>
    /// Groq provider configuration.
    /// </summary>
    public ProviderSettings Groq { get; init; } = new();

    /// <summary>
    /// Gemini provider configuration.
    /// </summary>
    public ProviderSettings Gemini { get; init; } = new();

    /// <summary>
    /// OpenRouter provider configuration.
    /// </summary>
    public ProviderSettings OpenRouter { get; init; } = new();
}

/// <summary>
/// Settings for a specific AI provider.
/// </summary>
public sealed class ProviderSettings
{
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = string.Empty;
    public string ModelName { get; init; } = string.Empty;
    public int MaxRequestsPerMinute { get; init; } = 60;
    public int MaxTokensPerRequest { get; init; } = 4096;
    public bool IsEnabled { get; init; } = true;
    public int Priority { get; init; } = 1;
    public int TimeoutSeconds { get; init; } = 30;
}
