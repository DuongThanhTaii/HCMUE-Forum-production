namespace UniHub.AI.Domain.Entities;

/// <summary>
/// Represents a single message in a conversation
/// </summary>
public class ConversationMessage
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Conversation this message belongs to
    /// </summary>
    public Guid ConversationId { get; set; }
    
    /// <summary>
    /// Who sent this message
    /// </summary>
    public MessageRole Role { get; set; }
    
    /// <summary>
    /// Message content
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional FAQ item that was used to generate this response
    /// </summary>
    public Guid? SourceFAQId { get; set; }
    
    /// <summary>
    /// Confidence score of the AI response (0-1)
    /// </summary>
    public double? ConfidenceScore { get; set; }
    
    /// <summary>
    /// Whether user found this response helpful
    /// </summary>
    public bool? IsHelpful { get; set; }
    
    /// <summary>
    /// When this message was sent
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Role of the message sender
/// </summary>
public enum MessageRole
{
    /// <summary>
    /// User asking question
    /// </summary>
    User,
    
    /// <summary>
    /// AI assistant (UniBot)
    /// </summary>
    Assistant,
    
    /// <summary>
    /// System message
    /// </summary>
    System,
    
    /// <summary>
    /// Human support agent
    /// </summary>
    SupportAgent
}
