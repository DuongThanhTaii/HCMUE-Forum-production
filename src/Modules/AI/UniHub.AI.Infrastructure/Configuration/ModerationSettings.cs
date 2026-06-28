namespace UniHub.AI.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for content moderation
/// </summary>
public class ModerationSettings
{
    public const string SectionName = "Moderation";
    
    /// <summary>
    /// Whether moderation is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Threshold for blocking content (0-1)
    /// </summary>
    public double BlockThreshold { get; set; } = 0.8;
    
    /// <summary>
    /// Threshold for flagging content for review (0-1)
    /// </summary>
    public double ReviewThreshold { get; set; } = 0.5;
    
    /// <summary>
    /// Threshold for considering content safe (below this = safe)
    /// </summary>
    public double SafeThreshold { get; set; } = 0.3;
    
    /// <summary>
    /// Whether to use AI for moderation
    /// </summary>
    public bool UseAI { get; set; } = true;
    
    /// <summary>
    /// Whether to use keyword-based detection
    /// </summary>
    public bool UseKeywordDetection { get; set; } = true;
    
    /// <summary>
    /// Spam detection settings
    /// </summary>
    public SpamDetectionSettings SpamDetection { get; set; } = new();
    
    /// <summary>
    /// Toxic content detection settings
    /// </summary>
    public ToxicContentSettings ToxicContent { get; set; } = new();
}

/// <summary>
/// Spam detection settings
/// </summary>
public class SpamDetectionSettings
{
    /// <summary>
    /// Maximum allowed URLs in content
    /// </summary>
    public int MaxUrls { get; set; } = 2;
    
    /// <summary>
    /// Maximum repeated characters threshold (e.g., "aaaaaaa")
    /// </summary>
    public int MaxRepeatedChars { get; set; } = 5;
    
    /// <summary>
    /// Maximum uppercase ratio (0-1)
    /// </summary>
    public double MaxUppercaseRatio { get; set; } = 0.7;
    
    /// <summary>
    /// Spam keywords to detect
    /// </summary>
    public List<string> SpamKeywords { get; set; } = new()
    {
        "buy now", "click here", "limited offer", "make money", "winner",
        "congratulations", "claim your", "free gift", "urgent", "act now"
    };
}

/// <summary>
/// Toxic content detection settings
/// </summary>
public class ToxicContentSettings
{
    /// <summary>
    /// Profanity keywords to detect (can be loaded from external source)
    /// </summary>
    public List<string> ProfanityKeywords { get; set; } = new()
    {
        // Basic examples - in production, use comprehensive profanity list
        "damn", "hell", "crap", "stupid", "idiot", "hate"
    };
    
    /// <summary>
    /// Hate speech indicators
    /// </summary>
    public List<string> HateSpeechKeywords { get; set; } = new()
    {
        // Examples - use comprehensive hate speech detection
        "racist", "kill yourself", "go die"
    };
}
