namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Request for document summarization
/// </summary>
public class SummarizationRequest
{
    /// <summary>
    /// Document content to summarize (plain text)
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional document URL (for reference)
    /// </summary>
    public string? DocumentUrl { get; set; }
    
    /// <summary>
    /// Optional document title
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Type of document being summarized
    /// </summary>
    public DocumentType DocumentType { get; set; } = DocumentType.Text;
    
    /// <summary>
    /// Desired summary length
    /// </summary>
    public SummaryLength Length { get; set; } = SummaryLength.Medium;
    
    /// <summary>
    /// Target language for summary (null = auto-detect from content)
    /// </summary>
    public string? TargetLanguage { get; set; }
    
    /// <summary>
    /// Source language (null = auto-detect)
    /// </summary>
    public string? SourceLanguage { get; set; }
    
    /// <summary>
    /// Maximum tokens for summary
    /// </summary>
    public int? MaxTokens { get; set; }
    
    /// <summary>
    /// Whether to cache the summary
    /// </summary>
    public bool EnableCaching { get; set; } = true;
    
    /// <summary>
    /// Optional user ID (for usage tracking)
    /// </summary>
    public Guid? UserId { get; set; }
}

/// <summary>
/// Type of document
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Plain text document
    /// </summary>
    Text,
    
    /// <summary>
    /// Academic paper or research document
    /// </summary>
    Academic,
    
    /// <summary>
    /// News article
    /// </summary>
    NewsArticle,
    
    /// <summary>
    /// Forum post or discussion thread
    /// </summary>
    ForumPost,
    
    /// <summary>
    /// Technical documentation
    /// </summary>
    Technical,
    
    /// <summary>
    /// General article or blog post
    /// </summary>
    Article
}

/// <summary>
/// Desired summary length
/// </summary>
public enum SummaryLength
{
    /// <summary>
    /// Very short summary (1-2 sentences, ~50-100 tokens)
    /// </summary>
    VeryShort,
    
    /// <summary>
    /// Short summary (2-3 sentences, ~100-200 tokens)
    /// </summary>
    Short,
    
    /// <summary>
    /// Medium summary (1 paragraph, ~200-400 tokens)
    /// </summary>
    Medium,
    
    /// <summary>
    /// Long summary (2-3 paragraphs, ~400-800 tokens)
    /// </summary>
    Long,
    
    /// <summary>
    /// Detailed summary (multiple paragraphs, ~800-1500 tokens)
    /// </summary>
    Detailed
}
