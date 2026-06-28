using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.DeleteMessage;

/// <summary>
/// Soft-delete (recall) a message; only the sender may delete.
/// </summary>
public sealed record DeleteMessageCommand(Guid MessageId, Guid UserId)
    : ICommand<DeleteMessageResult>;

public sealed record DeleteMessageResult(
    Guid ConversationId,
    Guid MessageId,
    DateTime DeletedAt);
