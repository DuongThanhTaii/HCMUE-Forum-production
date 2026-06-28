namespace UniHub.Chat.Domain.Messages;

/// <summary>
/// Loại message trong conversation
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Text message thông thường
    /// </summary>
    Text = 0,
    
    /// <summary>
    /// File attachment (documents, etc.)
    /// </summary>
    File = 1,
    
    /// <summary>
    /// Image attachment
    /// </summary>
    Image = 2,
    
    /// <summary>
    /// Video attachment
    /// </summary>
    Video = 3,
    
    /// <summary>
    /// System message (user joined, user left, etc.)
    /// </summary>
    System = 4,

    /// <summary>
    /// Missed call notification — persisted as a system-like message; content is caller display name.
    /// </summary>
    MissedCall = 5,

    /// <summary>
    /// Call ended — persisted when the caller or callee ends a connected call.
    /// </summary>
    CallEnded = 6
}
