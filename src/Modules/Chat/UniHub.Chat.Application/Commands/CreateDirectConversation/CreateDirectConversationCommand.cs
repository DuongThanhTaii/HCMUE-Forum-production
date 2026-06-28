using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.CreateDirectConversation;

/// <summary>
/// Command to create a new direct (1:1) conversation
/// </summary>
public sealed record CreateDirectConversationCommand(
    Guid User1Id,
    Guid User2Id,
    Guid CreatorId) : ICommand<Guid>;
