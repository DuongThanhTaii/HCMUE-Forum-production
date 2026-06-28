namespace UniHub.Chat.Domain.Conversations;

/// <summary>
/// Loại conversation
/// </summary>
public enum ConversationType
{
    /// <summary>
    /// Chat 1:1 giữa 2 người (exactly 2 participants)
    /// </summary>
    Direct = 0,

    /// <summary>
    /// Group chat với nhiều người (2+ participants)
    /// </summary>
    Group = 1
}
