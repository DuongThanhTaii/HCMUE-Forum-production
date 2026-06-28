namespace UniHub.AI.Domain.Entities;

/// <summary>
/// Represents a Frequently Asked Question in the knowledge base
/// </summary>
public class FAQItem
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// The question text
    /// </summary>
    public string Question { get; set; } = string.Empty;
    
    /// <summary>
    /// The answer text
    /// </summary>
    public string Answer { get; set; } = string.Empty;
    
    /// <summary>
    /// Category of the FAQ (e.g., "Admission", "Registration", "Campus Life")
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Tags for better searchability
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Priority for ranking search results (higher = more important)
    /// </summary>
    public int Priority { get; set; } = 0;
    
    /// <summary>
    /// Whether this FAQ is active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Number of times this FAQ was used
    /// </summary>
    public int UsageCount { get; set; } = 0;
    
    /// <summary>
    /// Average rating from users (1-5)
    /// </summary>
    public double? AverageRating { get; set; }
    
    /// <summary>
    /// When this FAQ was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this FAQ was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
