namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Conversation data transfer object
/// </summary>
public class ConversationDto
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? Title { get; set; }
    public List<ConversationMessageDto> Messages { get; set; } = new();
    public bool HandedOffToSupport { get; set; }
    public string? HandoffReason { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime LastActiveAt { get; set; }
    public bool IsClosed { get; set; }
}

/// <summary>
/// Conversation message data transfer object
/// </summary>
public class ConversationMessageDto
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? SourceFAQId { get; set; }
    public double? ConfidenceScore { get; set; }
    public bool? IsHelpful { get; set; }
    public DateTime SentAt { get; set; }
}
