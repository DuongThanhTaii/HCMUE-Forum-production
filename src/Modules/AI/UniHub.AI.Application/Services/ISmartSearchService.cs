using UniHub.AI.Application.DTOs;

namespace UniHub.AI.Application.Services;

/// <summary>
/// Service for AI-powered smart search with semantic understanding.
/// </summary>
public interface ISmartSearchService
{
    /// <summary>
    /// Perform smart search with AI-enhanced query understanding.
    /// </summary>
    /// <param name="request">Search request with query and filters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results with relevance ranking and suggestions.</returns>
    Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get search suggestions based on partial query.
    /// </summary>
    /// <param name="partialQuery">Partial query text.</param>
    /// <param name="limit">Maximum number of suggestions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of suggested queries.</returns>
    Task<List<string>> GetSuggestionsAsync(string partialQuery, int limit = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Understand query intent and expand with AI.
    /// </summary>
    /// <param name="query">Original query text.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Query understanding with intent and expansions.</returns>
    Task<QueryUnderstanding> UnderstandQueryAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate relevance score between query and content.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="content">Content to score.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Relevance score (0.0 to 1.0).</returns>
    Task<double> CalculateRelevanceAsync(string query, string content, CancellationToken cancellationToken = default);
}
