namespace UniHub.AI.Domain.AIProviders;

/// <summary>
/// Type of AI provider for making API calls.
/// Supports multiple providers with automatic fallback.
/// </summary>
public enum AIProviderType
{
    /// <summary>Groq AI - Fast inference, free tier available.</summary>
    Groq = 1,

    /// <summary>Google Gemini - Google's AI model.</summary>
    Gemini = 2,

    /// <summary>OpenRouter - Unified API for multiple AI models.</summary>
    OpenRouter = 3,

    /// <summary>OpenAI - GPT models (if needed in future).</summary>
    OpenAI = 4
}
