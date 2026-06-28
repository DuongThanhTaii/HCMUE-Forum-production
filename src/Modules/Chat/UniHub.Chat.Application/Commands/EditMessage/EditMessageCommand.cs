using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.EditMessage;

/// <summary>
/// Command to edit an existing message (sender only).
/// </summary>
public sealed record EditMessageCommand(Guid MessageId, Guid UserId, string Content)
    : ICommand<EditMessageResult>;

/// <summary>
/// Result returned after a successful edit (for SignalR + API clients).
/// </summary>
public sealed record EditMessageResult(
    Guid ConversationId,
    Guid MessageId,
    string Content,
    DateTime EditedAt);
