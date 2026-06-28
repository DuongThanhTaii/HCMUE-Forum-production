namespace UniHub.AI.Domain.Entities;

/// <summary>
/// Cached summary entry
/// </summary>
public class SummaryCacheEntry
{
    public string CacheKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Generated summary text
    /// </summary>
    public string Summary { get; set; } = string.Empty;
    
    /// <summary>
    /// Key points extracted
    /// </summary>
    public List<string> KeyPoints { get; set; } = new();
    
    /// <summary>
    /// Detected language
    /// </summary>
    public string? DetectedLanguage { get; set; }
    
    /// <summary>
    /// Original content length
    /// </summary>
    public int OriginalLength { get; set; }
    
    /// <summary>
    /// Summary length
    /// </summary>
    public int SummaryLength { get; set; }
    
    /// <summary>
    /// Compression ratio
    /// </summary>
    public double CompressionRatio { get; set; }
    
    /// <summary>
    /// Tokens used for generation
    /// </summary>
    public int? TokensUsed { get; set; }
    
    /// <summary>
    /// When this was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this expires (null = never)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Number of times this cache entry was accessed
    /// </summary>
    public int AccessCount { get; set; } = 0;
    
    /// <summary>
    /// Last time this was accessed
    /// </summary>
    public DateTime? LastAccessedAt { get; set; }
}
