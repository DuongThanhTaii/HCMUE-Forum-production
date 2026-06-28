using UniHub.AI.Application.DTOs;

namespace UniHub.AI.Application.Services;

/// <summary>
/// Service for document summarization using AI
/// </summary>
public interface IDocumentSummarizationService
{
    /// <summary>
    /// Summarize a document
    /// </summary>
    Task<SummarizationResponse> SummarizeAsync(SummarizationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Extract key points from document (without full summary)
    /// </summary>
    Task<List<string>> ExtractKeyPointsAsync(string content, int maxPoints = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detect language of document
    /// </summary>
    Task<string> DetectLanguageAsync(string content, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Clear cache for specific document
    /// </summary>
    Task ClearCacheAsync(string cacheKey);
    
    /// <summary>
    /// Clear all summaries cache
    /// </summary>
    Task ClearAllCacheAsync();
}
