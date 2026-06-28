using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Commands.SendMessage;

/// <summary>
/// Command to send a text message in a conversation
/// </summary>
public sealed record SendMessageCommand(
    Guid ConversationId,
    Guid SenderId,
    string Content,
    Guid? ReplyToMessageId = null) : ICommand<Guid>;
