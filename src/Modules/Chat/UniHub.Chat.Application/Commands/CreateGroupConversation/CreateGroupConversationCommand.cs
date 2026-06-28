using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.CreateGroupConversation;

/// <summary>
/// Command to create a group conversation
/// </summary>
public sealed record CreateGroupConversationCommand(
    string Title,
    List<Guid> ParticipantIds,
    Guid CreatorId) : ICommand<Guid>;
