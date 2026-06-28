using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.JoinChannel;

/// <summary>
/// Command to join a channel
/// </summary>
public sealed record JoinChannelCommand(
    Guid ChannelId,
    Guid UserId) : ICommand;
