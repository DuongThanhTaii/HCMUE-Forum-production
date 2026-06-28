using System.Text.Json;

namespace UniHub.Chat.Infrastructure.Persistence.Repositories;

/// <summary>
/// Builds PostgreSQL jsonb containment payloads for <c>participants @> ...</c> filters.
/// </summary>
internal static class ConversationParticipantQuery
{
    public static string ContainsPayload(Guid userId) =>
        JsonSerializer.Serialize(new[] { userId });
}
