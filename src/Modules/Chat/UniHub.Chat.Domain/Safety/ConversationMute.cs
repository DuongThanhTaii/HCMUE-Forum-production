namespace UniHub.Chat.Domain.Safety;

public sealed class ConversationMute
{
    public Guid UserId { get; private set; }
    public Guid ConversationId { get; private set; }
    public bool IsMuted { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ConversationMute() { }

    public static ConversationMute Create(Guid userId, Guid conversationId, bool isMuted)
    {
        return new ConversationMute
        {
            UserId = userId,
            ConversationId = conversationId,
            IsMuted = isMuted,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void SetMuted(bool isMuted)
    {
        IsMuted = isMuted;
        UpdatedAt = DateTime.UtcNow;
    }
}
