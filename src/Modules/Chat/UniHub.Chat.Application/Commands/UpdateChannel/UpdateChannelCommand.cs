using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.UpdateChannel;

/// <summary>
/// Command to update channel information
/// </summary>
public sealed record UpdateChannelCommand(
    Guid ChannelId,
    string? Name,
    string? Description,
    Guid UpdatedBy) : ICommand;
