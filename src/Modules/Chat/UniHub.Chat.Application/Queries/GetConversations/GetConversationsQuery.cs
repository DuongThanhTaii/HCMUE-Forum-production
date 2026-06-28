using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Queries.GetConversations;

/// <summary>
/// Response for a conversation in the list
/// </summary>
public sealed record ConversationResponse(
    Guid Id,
    string Type,
    List<Guid> ParticipantIds,
    DateTime? LastMessageAt,
    DateTime CreatedAt,
    bool IsArchived,
    string? Title,
    Guid? DirectPeerUserId,
    string? DirectPeerFullName,
    string? DirectPeerEmail,
    bool IsMuted,
    bool IsBlockedWithPeer);

/// <summary>
/// Query to get all conversations for a user
/// </summary>
public sealed record GetConversationsQuery(Guid UserId) : IQuery<IReadOnlyList<ConversationResponse>>;
