using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Conversations;

/// <summary>
/// Strongly-typed ID for Conversation aggregate
/// </summary>
public sealed record ConversationId(Guid Value) : GuidId(Value)
{
    public static ConversationId Create(Guid value) => new(value);
    public static ConversationId CreateUnique() => new(Guid.NewGuid());
}
