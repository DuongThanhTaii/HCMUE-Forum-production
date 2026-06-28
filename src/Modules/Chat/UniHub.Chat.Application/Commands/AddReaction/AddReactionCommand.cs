using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.AddReaction;

/// <summary>
/// Command to add an emoji reaction to a message
/// </summary>
public sealed record AddReactionCommand(
    Guid MessageId,
    Guid UserId,
    string Emoji) : ICommand;
