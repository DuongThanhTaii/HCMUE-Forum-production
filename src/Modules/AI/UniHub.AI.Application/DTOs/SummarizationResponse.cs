namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Response from document summarization
/// </summary>
public class SummarizationResponse
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Generated summary
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    
    /// <summary>
    /// Detected source language
    /// </summary>
    public string? DetectedLanguage { get; set; }
    
    /// <summary>
    /// Key points extracted from document
    /// </summary>
    public List<string> KeyPoints { get; set; } = new();
    
    /// <summary>
    /// Original document length (characters)
    /// </summary>
    public int OriginalLength { get; set; }
    
    /// <summary>
    /// Summary length (characters)
    /// </summary>
    public int SummaryLength { get; set; }
    
    /// <summary>
    /// Compression ratio (summary length / original length)
    /// </summary>
    public double CompressionRatio { get; set; }
    
    /// <summary>
    /// Tokens used for summarization
    /// </summary>
    public int? TokensUsed { get; set; }
    
    /// <summary>
    /// Whether this result was retrieved from cache
    /// </summary>
    public bool FromCache { get; set; }
    
    /// <summary>
    /// Cache key (if cached)
    /// </summary>
    public string? CacheKey { get; set; }
    
    /// <summary>
    /// Error message (if unsuccessful)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Timestamp of summarization
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Processing time in milliseconds
    /// </summary>
    public long ProcessingTimeMs { get; set; }
}
