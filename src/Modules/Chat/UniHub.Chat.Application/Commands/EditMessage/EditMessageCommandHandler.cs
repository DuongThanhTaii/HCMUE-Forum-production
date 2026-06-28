using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Messages;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.EditMessage;

public sealed class EditMessageCommandHandler : ICommandHandler<EditMessageCommand, EditMessageResult>
{
    private readonly IMessageRepository _messageRepository;

    public EditMessageCommandHandler(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<Result<EditMessageResult>> Handle(
        EditMessageCommand request,
        CancellationToken cancellationToken)
    {
        var messageId = MessageId.Create(request.MessageId);
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure<EditMessageResult>(new Error(
                "Message.NotFound",
                $"Message {request.MessageId} was not found"));
        }

        var edit = message.Edit(request.Content, request.UserId);
        if (edit.IsFailure)
        {
            return Result.Failure<EditMessageResult>(edit.Error);
        }

        await _messageRepository.UpdateAsync(message, cancellationToken);

        var editedAt = message.EditedAt ?? DateTime.UtcNow;

        return Result.Success(new EditMessageResult(
            message.ConversationId.Value,
            message.Id.Value,
            message.Content,
            editedAt));
    }
}
