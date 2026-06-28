namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Request for content moderation
/// </summary>
public class ModerationRequest
{
    /// <summary>
    /// Content to moderate
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of content being moderated
    /// </summary>
    public ContentType ContentType { get; set; } = ContentType.Text;
    
    /// <summary>
    /// Optional user ID (for tracking repeat offenders)
    /// </summary>
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// Optional context about where content will be posted
    /// </summary>
    public string? Context { get; set; }
}

/// <summary>
/// Type of content being moderated
/// </summary>
public enum ContentType
{
    /// <summary>
    /// Plain text (posts, comments, etc.)
    /// </summary>
    Text,
    
    /// <summary>
    /// Profile information (bio, username, etc.)
    /// </summary>
    Profile,
    
    /// <summary>
    /// Private messages
    /// </summary>
    DirectMessage,
    
    /// <summary>
    /// Forum posts
    /// </summary>
    ForumPost,
    
    /// <summary>
    /// Comments
    /// </summary>
    Comment
}
