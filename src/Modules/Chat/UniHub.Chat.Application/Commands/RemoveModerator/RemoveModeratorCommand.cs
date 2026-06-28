using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.RemoveModerator;

/// <summary>
/// Command to remove a moderator from a channel
/// </summary>
public sealed record RemoveModeratorCommand(
    Guid ChannelId,
    Guid UserId,
    Guid RemovedBy) : ICommand;
