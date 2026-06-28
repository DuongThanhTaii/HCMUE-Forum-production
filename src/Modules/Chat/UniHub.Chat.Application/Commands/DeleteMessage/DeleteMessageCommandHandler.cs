using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Messages;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.DeleteMessage;

public sealed class DeleteMessageCommandHandler : ICommandHandler<DeleteMessageCommand, DeleteMessageResult>
{
    private readonly IMessageRepository _messageRepository;

    public DeleteMessageCommandHandler(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<Result<DeleteMessageResult>> Handle(
        DeleteMessageCommand request,
        CancellationToken cancellationToken)
    {
        var messageId = MessageId.Create(request.MessageId);
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure<DeleteMessageResult>(new Error(
                "Message.NotFound",
                $"Message {request.MessageId} was not found"));
        }

        var del = message.Delete(request.UserId);
        if (del.IsFailure)
        {
            return Result.Failure<DeleteMessageResult>(del.Error);
        }

        await _messageRepository.UpdateAsync(message, cancellationToken);

        var deletedAt = message.DeletedAt ?? DateTime.UtcNow;

        return Result.Success(new DeleteMessageResult(
            message.ConversationId.Value,
            message.Id.Value,
            deletedAt));
    }
}
