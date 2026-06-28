using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.LeaveChannel;

/// <summary>
/// Command to leave a channel
/// </summary>
public sealed record LeaveChannelCommand(
    Guid ChannelId,
    Guid UserId) : ICommand;
