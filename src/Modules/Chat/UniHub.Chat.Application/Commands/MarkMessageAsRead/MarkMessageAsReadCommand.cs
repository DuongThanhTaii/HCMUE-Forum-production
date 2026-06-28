using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.MarkMessageAsRead;

/// <summary>
/// Command to mark a message as read by a user
/// </summary>
public sealed record MarkMessageAsReadCommand(
    Guid MessageId,
    Guid UserId) : ICommand;
