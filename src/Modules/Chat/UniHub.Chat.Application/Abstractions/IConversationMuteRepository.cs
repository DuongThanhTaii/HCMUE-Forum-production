using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Safety;

namespace UniHub.Chat.Application.Abstractions;

public interface IConversationMuteRepository
{
    Task<ConversationMute?> GetAsync(
        Guid userId,
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, bool>> GetMuteStatesAsync(
        Guid userId,
        IReadOnlyList<Guid> conversationIds,
        CancellationToken cancellationToken = default);

    Task UpsertAsync(ConversationMute mute, CancellationToken cancellationToken = default);
}
