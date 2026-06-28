namespace UniHub.AI.Domain.Entities;

/// <summary>
/// Represents a conversation session with UniBot
/// </summary>
public class Conversation
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// User ID who owns this conversation
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Optional session ID for anonymous users
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Title of the conversation (generated from first message)
    /// </summary>
    public string? Title { get; set; }
    
    /// <summary>
    /// Messages in this conversation
    /// </summary>
    public List<ConversationMessage> Messages { get; set; } = new();
    
    /// <summary>
    /// Whether conversation was handed off to human support
    /// </summary>
    public bool HandedOffToSupport { get; set; } = false;
    
    /// <summary>
    /// Reason for handoff (if applicable)
    /// </summary>
    public string? HandoffReason { get; set; }
    
    /// <summary>
    /// Support agent ID who took over (if applicable)
    /// </summary>
    public Guid? SupportAgentId { get; set; }
    
    /// <summary>
    /// When this conversation started
    /// </summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When this conversation was last active
    /// </summary>
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Whether this conversation is closed
    /// </summary>
    public bool IsClosed { get; set; } = false;
    
    /// <summary>
    /// When this conversation was closed (if applicable)
    /// </summary>
    public DateTime? ClosedAt { get; set; }
}
