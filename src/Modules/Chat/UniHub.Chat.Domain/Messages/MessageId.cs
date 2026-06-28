using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Messages;

/// <summary>
/// Strongly-typed identifier cho Message entity
/// </summary>
public sealed record MessageId(Guid Value) : GuidId(Value)
{
    public static MessageId Create(Guid value) => new(value);
    public static MessageId CreateUnique() => new(Guid.NewGuid());
}
