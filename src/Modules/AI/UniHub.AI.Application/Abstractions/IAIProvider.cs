using UniHub.AI.Domain.AIProviders;

namespace UniHub.AI.Application.Abstractions;

/// <summary>
/// Request model for AI chat/completion.
/// </summary>
public sealed class AIRequest
{
    /// <summary>
    /// User's input prompt/message.
    /// </summary>
    public string Prompt { get; init; } = string.Empty;

    /// <summary>
    /// System message to set context/behavior.
    /// </summary>
    public string? SystemMessage { get; init; }

    /// <summary>
    /// Maximum tokens to generate in response.
    /// </summary>
    public int MaxTokens { get; init; } = 1024;

    /// <summary>
    /// Temperature for response randomness (0.0 to 1.0).
    /// </summary>
    public double Temperature { get; init; } = 0.7;

    /// <summary>
    /// Optional conversation history for context.
    /// </summary>
    public List<ChatMessage>? ConversationHistory { get; init; }
}

/// <summary>
/// Chat message for conversation history.
/// </summary>
public sealed class ChatMessage
{
    /// <summary>
    /// Role of the message sender (user, assistant, system).
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// Content of the message.
    /// </summary>
    public string Content { get; init; } = string.Empty;
}

/// <summary>
/// Response model from AI provider.
/// </summary>
public sealed class AIResponse
{
    /// <summary>
    /// Generated text response.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Provider that generated this response.
    /// </summary>
    public AIProviderType ProviderType { get; init; }

    /// <summary>
    /// Tokens used in this request (input + output).
    /// </summary>
    public int TokensUsed { get; init; }

    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Error message if request failed.
    /// </summary>
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Interface for AI provider implementations.
/// Each provider (Groq, Gemini, OpenRouter) implements this contract.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Type of this provider.
    /// </summary>
    AIProviderType ProviderType { get; }

    /// <summary>
    /// Configuration for this provider.
    /// </summary>
    AIProviderConfiguration Configuration { get; }

    /// <summary>
    /// Check if provider is available (has quota remaining).
    /// </summary>
    /// <returns>True if provider can accept requests.</returns>
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Send a chat completion request to the AI provider.
    /// </summary>
    /// <param name="request">Request containing prompt and parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>AI response with generated content.</returns>
    Task<AIResponse> SendChatRequestAsync(AIRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get remaining quota for this provider.
    /// </summary>
    /// <returns>Remaining requests allowed in current time window.</returns>
    Task<int> GetRemainingQuotaAsync(CancellationToken cancellationToken = default);
}
