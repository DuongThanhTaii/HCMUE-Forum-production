using UniHub.SharedKernel.Domain;

namespace UniHub.Chat.Domain.Channels;

/// <summary>
/// Strongly-typed identifier cho Channel aggregate
/// </summary>
public sealed record ChannelId(Guid Value) : GuidId(Value)
{
    public static ChannelId Create(Guid value) => new(value);
    public static ChannelId CreateUnique() => new(Guid.NewGuid());
}
