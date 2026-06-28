using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.AddModerator;

/// <summary>
/// Command to add a moderator to a channel
/// </summary>
public sealed record AddModeratorCommand(
    Guid ChannelId,
    Guid UserId,
    Guid AddedBy) : ICommand;
