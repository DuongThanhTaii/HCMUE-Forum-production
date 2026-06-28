using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.RemoveReaction;

/// <summary>
/// Command to remove an emoji reaction from a message
/// </summary>
public sealed record RemoveReactionCommand(
    Guid MessageId,
    Guid UserId,
    string Emoji) : ICommand;
