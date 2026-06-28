using UniHub.AI.Application.Abstractions;
using UniHub.AI.Domain.AIProviders;

namespace UniHub.AI.Infrastructure.Providers;

/// <summary>
/// Factory for creating and managing AI provider instances.
/// Handles provider selection and fallback logic.
/// </summary>
public interface IAIProviderFactory
{
    /// <summary>
    /// Get the best available AI provider based on priority and availability.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>First available provider, or null if none available.</returns>
    Task<IAIProvider?> GetAvailableProviderAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a specific provider by type.
    /// </summary>
    /// <param name="providerType">Type of provider to retrieve.</param>
    /// <returns>Provider instance, or null if not found.</returns>
    IAIProvider? GetProvider(AIProviderType providerType);

    /// <summary>
    /// Get all registered providers ordered by priority.
    /// </summary>
    /// <returns>List of all providers.</returns>
    IReadOnlyList<IAIProvider> GetAllProviders();
}

/// <summary>
/// Factory implementation for AI providers.
/// </summary>
public sealed class AIProviderFactory : IAIProviderFactory
{
    private readonly IReadOnlyList<IAIProvider> _providers;

    public AIProviderFactory(IEnumerable<IAIProvider> providers)
    {
        // Sort providers by priority (lower number = higher priority)
        _providers = providers
            .Where(p => p.Configuration.IsEnabled)
            .OrderBy(p => p.Configuration.Priority)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<IAIProvider?> GetAvailableProviderAsync(CancellationToken cancellationToken = default)
    {
        // Try each provider in priority order
        foreach (var provider in _providers)
        {
            if (await provider.IsAvailableAsync(cancellationToken))
            {
                return provider;
            }
        }

        // No provider available
        return null;
    }

    /// <inheritdoc />
    public IAIProvider? GetProvider(AIProviderType providerType)
    {
        return _providers.FirstOrDefault(p => p.ProviderType == providerType);
    }

    /// <inheritdoc />
    public IReadOnlyList<IAIProvider> GetAllProviders()
    {
        return _providers;
    }
}
