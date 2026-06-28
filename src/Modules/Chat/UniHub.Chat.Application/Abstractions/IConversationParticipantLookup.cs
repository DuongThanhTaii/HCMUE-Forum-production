namespace UniHub.Chat.Application.Abstractions;

/// <summary>
/// Resolves identity display fields for conversation participants (direct chat peer labels).
/// </summary>
public interface IConversationParticipantLookup
{
    Task<IReadOnlyDictionary<Guid, ParticipantDisplay>> GetByIdsAsync(
        IReadOnlyList<Guid> userIds,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Minimal user info for chat UI labels.
/// </summary>
public readonly record struct ParticipantDisplay(string FullName, string Email);
