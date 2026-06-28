namespace UniHub.AI.Domain.AIProviders;

/// <summary>
/// Configuration for an AI provider.
/// Contains API credentials, base URLs, and quota limits.
/// </summary>
public sealed class AIProviderConfiguration
{
    /// <summary>
    /// Type of AI provider (Groq, Gemini, OpenRouter, etc.)
    /// </summary>
    public AIProviderType ProviderType { get; init; }

    /// <summary>
    /// API key for authenticating with the provider.
    /// </summary>
    public string ApiKey { get; init; } = string.Empty;

    /// <summary>
    /// Base URL for API requests.
    /// </summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>
    /// Model name/identifier to use (e.g., "llama-3.1-70b", "gemini-pro").
    /// </summary>
    public string ModelName { get; init; } = string.Empty;

    /// <summary>
    /// Maximum requests per minute.
    /// </summary>
    public int MaxRequestsPerMinute { get; init; } = 60;

    /// <summary>
    /// Maximum tokens per request.
    /// </summary>
    public int MaxTokensPerRequest { get; init; } = 4096;

    /// <summary>
    /// Whether this provider is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Priority order (lower number = higher priority).
    /// Used for fallback routing.
    /// </summary>
    public int Priority { get; init; } = 1;

    /// <summary>
    /// Timeout in seconds for API requests.
    /// </summary>
    public int TimeoutSeconds { get; init; } = 30;
}
