namespace UniHub.Chat.Domain.Channels;

/// <summary>
/// Loại channel (Public/Private)
/// </summary>
public enum ChannelType
{
    /// <summary>
    /// Public channel - anyone can discover and join
    /// </summary>
    Public = 0,
    
    /// <summary>
    /// Private channel - invite only
    /// </summary>
    Private = 1
}
