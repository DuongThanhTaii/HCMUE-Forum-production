using UniHub.AI.Application.Abstractions;
using UniHub.AI.Domain.AIProviders;

namespace UniHub.AI.Infrastructure.Providers;

/// <summary>
/// Base class for AI provider implementations.
/// Handles common functionality like quota tracking and configuration.
/// </summary>
public abstract class AIProviderBase : IAIProvider
{
    private readonly SemaphoreSlim _rateLimitSemaphore;
    private readonly Queue<DateTime> _requestTimestamps;
    private readonly object _lock = new();

    protected AIProviderBase(AIProviderConfiguration configuration)
    {
        Configuration = configuration;
        _rateLimitSemaphore = new SemaphoreSlim(1, 1);
        _requestTimestamps = new Queue<DateTime>();
    }

    /// <inheritdoc />
    public abstract AIProviderType ProviderType { get; }

    /// <inheritdoc />
    public AIProviderConfiguration Configuration { get; }

    /// <inheritdoc />
    public virtual async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        if (!Configuration.IsEnabled)
        {
            return false;
        }

        // Check if we have quota remaining
        var remainingQuota = await GetRemainingQuotaAsync(cancellationToken);
        return remainingQuota > 0;
    }

    /// <inheritdoc />
    public abstract Task<AIResponse> SendChatRequestAsync(AIRequest request, CancellationToken cancellationToken = default);

    /// <inheritdoc />
    public Task<int> GetRemainingQuotaAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            // Remove timestamps older than 1 minute
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
            while (_requestTimestamps.Count > 0 && _requestTimestamps.Peek() < oneMinuteAgo)
            {
                _requestTimestamps.Dequeue();
            }

            // Calculate remaining quota
            var used = _requestTimestamps.Count;
            var remaining = Math.Max(0, Configuration.MaxRequestsPerMinute - used);
            return Task.FromResult(remaining);
        }
    }

    /// <summary>
    /// Record a request for rate limiting purposes.
    /// Call this before making actual API request.
    /// </summary>
    protected async Task<bool> TryAcquireRequestSlotAsync(CancellationToken cancellationToken)
    {
        await _rateLimitSemaphore.WaitAsync(cancellationToken);
        try
        {
            var remaining = await GetRemainingQuotaAsync(cancellationToken);
            if (remaining <= 0)
            {
                return false;
            }

            lock (_lock)
            {
                _requestTimestamps.Enqueue(DateTime.UtcNow);
            }

            return true;
        }
        finally
        {
            _rateLimitSemaphore.Release();
        }
    }

    /// <summary>
    /// Create a failure response.
    /// </summary>
    protected AIResponse CreateFailureResponse(string errorMessage)
    {
        return new AIResponse
        {
            Content = string.Empty,
            ProviderType = ProviderType,
            TokensUsed = 0,
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }

    /// <summary>
    /// Create a success response.
    /// </summary>
    protected AIResponse CreateSuccessResponse(string content, int tokensUsed)
    {
        return new AIResponse
        {
            Content = content,
            ProviderType = ProviderType,
            TokensUsed = tokensUsed,
            IsSuccess = true,
            ErrorMessage = null
        };
    }
}
