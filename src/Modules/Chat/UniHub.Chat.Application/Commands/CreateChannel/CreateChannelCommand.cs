using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.CreateChannel;

/// <summary>
/// Command to create a new channel
/// </summary>
public sealed record CreateChannelCommand(
    string Name,
    string? Description,
    bool IsPublic,
    Guid OwnerId) : ICommand<Guid>;
