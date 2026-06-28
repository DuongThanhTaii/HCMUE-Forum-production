using System.Text.Json;
using UniHub.Chat.Domain.Channels;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;

namespace UniHub.Chat.Infrastructure.Persistence.Configurations;

internal static class ChatModelConversion
{
    public static ConversationId ToConversationId(Guid value)
    {
        return ConversationId.Create(value);
    }

    public static ChannelId ToChannelId(Guid value)
    {
        return ChannelId.Create(value);
    }

    public static MessageId ToMessageId(Guid value)
    {
        return MessageId.Create(value);
    }

    public static string ToGuidListDb(List<Guid> values)
    {
        return JsonSerializer.Serialize(values);
    }

    public static List<Guid> ToGuidListDomain(string raw)
    {
        return JsonSerializer.Deserialize<List<Guid>>(raw) ?? new List<Guid>();
    }
}
