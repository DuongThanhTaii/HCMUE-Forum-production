namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Response from content moderation
/// </summary>
public class ModerationResponse
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// Whether content is safe (passed moderation)
    /// </summary>
    public bool IsSafe { get; set; }
    
    /// <summary>
    /// Whether content should be flagged for manual review
    /// </summary>
    public bool RequiresReview { get; set; }
    
    /// <summary>
    /// Whether content should be automatically blocked
    /// </summary>
    public bool IsBlocked { get; set; }
    
    /// <summary>
    /// Overall confidence score (0-1)
    /// </summary>
    public double ConfidenceScore { get; set; }
    
    /// <summary>
    /// Detected violations
    /// </summary>
    public List<ModerationViolation> Violations { get; set; } = new();
    
    /// <summary>
    /// Reason for moderation decision
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// Error message (if unsuccessful)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Timestamp of moderation
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A detected moderation violation
/// </summary>
public class ModerationViolation
{
    /// <summary>
    /// Type of violation
    /// </summary>
    public ViolationType Type { get; set; }
    
    /// <summary>
    /// Severity level (0-1, higher = more severe)
    /// </summary>
    public double Severity { get; set; }
    
    /// <summary>
    /// Confidence score (0-1)
    /// </summary>
    public double Confidence { get; set; }
    
    /// <summary>
    /// Description of the violation
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Types of content violations
/// </summary>
public enum ViolationType
{
    /// <summary>
    /// Toxic or abusive language
    /// </summary>
    Toxic,
    
    /// <summary>
    /// Hate speech
    /// </summary>
    HateSpeech,
    
    /// <summary>
    /// Harassment or bullying
    /// </summary>
    Harassment,
    
    /// <summary>
    /// Sexual content
    /// </summary>
    Sexual,
    
    /// <summary>
    /// Violence or threats
    /// </summary>
    Violence,
    
    /// <summary>
    /// Spam or advertising
    /// </summary>
    Spam,
    
    /// <summary>
    /// Profanity
    /// </summary>
    Profanity,
    
    /// <summary>
    /// Personal information (doxxing)
    /// </summary>
    PersonalInfo,
    
    /// <summary>
    /// Misinformation
    /// </summary>
    Misinformation,
    
    /// <summary>
    /// Other violation
    /// </summary>
    Other
}
