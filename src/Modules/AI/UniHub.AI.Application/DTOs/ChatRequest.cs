namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Request for chatting with UniBot
/// </summary>
public class ChatRequest
{
    /// <summary>
    /// User's message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional conversation ID for continuing existing conversation
    /// </summary>
    public Guid? ConversationId { get; set; }
    
    /// <summary>
    /// Optional user ID (for authenticated users)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Optional session ID (for anonymous users)
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Whether to include conversation history in AI request
    /// </summary>
    public bool IncludeHistory { get; set; } = true;
    
    /// <summary>
    /// Maximum number of previous messages to include (default: 10)
    /// </summary>
    public int MaxHistoryMessages { get; set; } = 10;
}
