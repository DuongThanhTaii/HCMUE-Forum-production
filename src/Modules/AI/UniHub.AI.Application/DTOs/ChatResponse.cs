namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Response from UniBot
/// </summary>
public class ChatResponse
{
    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }
    
    /// <summary>
    /// UniBot's response message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Conversation ID
    /// </summary>
    public Guid ConversationId { get; set; }
    
    /// <summary>
    /// Message ID
    /// </summary>
    public Guid MessageId { get; set; }
    
    /// <summary>
    /// Confidence score of the response (0-1)
    /// </summary>
    public double? ConfidenceScore { get; set; }
    
    /// <summary>
    /// FAQ item used to generate response (if any)
    /// </summary>
    public FAQItemDto? SourceFAQ { get; set; }
    
    /// <summary>
    /// Suggested questions for follow-up
    /// </summary>
    public List<string> SuggestedQuestions { get; set; } = new();
    
    /// <summary>
    /// Whether handoff to human support is suggested
    /// </summary>
    public bool SuggestHandoff { get; set; }
    
    /// <summary>
    /// Reason for suggested handoff
    /// </summary>
    public string? HandoffReason { get; set; }
    
    /// <summary>
    /// Error message (if unsuccessful)
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Timestamp of response
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
