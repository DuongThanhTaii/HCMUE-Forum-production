namespace UniHub.AI.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for document summarization
/// </summary>
public class SummarizationSettings
{
    public const string SectionName = "Summarization";
    
    /// <summary>
    /// Whether summarization is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Whether to enable caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;
    
    /// <summary>
    /// Cache expiration in hours
    /// </summary>
    public int CacheExpirationHours { get; set; } = 24;
    
    /// <summary>
    /// Maximum document length to summarize (characters)
    /// </summary>
    public int MaxDocumentLength { get; set; } = 50000;
    
    /// <summary>
    /// Minimum document length to summarize (characters)
    /// </summary>
    public int MinDocumentLength { get; set; } = 100;
    
    /// <summary>
    /// Default summary length
    /// </summary>
    public string DefaultLength { get; set; } = "Medium";
    
    /// <summary>
    /// Token limits for different summary lengths
    /// </summary>
    public SummaryLengthTokens LengthTokens { get; set; } = new();
    
    /// <summary>
    /// Supported languages
    /// </summary>
    public List<string> SupportedLanguages { get; set; } = new()
    {
        "en", "vi", "zh", "ja", "ko", "fr", "de", "es", "pt", "ru"
    };
    
    /// <summary>
    /// Default language when detection fails
    /// </summary>
    public string DefaultLanguage { get; set; } = "en";
}

/// <summary>
/// Token limits for summary lengths
/// </summary>
public class SummaryLengthTokens
{
    public int VeryShort { get; set; } = 100;
    public int Short { get; set; } = 200;
    public int Medium { get; set; } = 400;
    public int Long { get; set; } = 800;
    public int Detailed { get; set; } = 1500;
}
