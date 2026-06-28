using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Messages;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.MarkMessageAsRead;

/// <summary>
/// Handler for marking a message as read
/// </summary>
public sealed class MarkMessageAsReadCommandHandler : ICommandHandler<MarkMessageAsReadCommand>
{
    private readonly IMessageRepository _messageRepository;

    public MarkMessageAsReadCommandHandler(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<Result> Handle(
        MarkMessageAsReadCommand request,
        CancellationToken cancellationToken)
    {
        var messageId = MessageId.Create(request.MessageId);

        var added = await _messageRepository.TryAddReadReceiptAsync(
            messageId,
            request.UserId,
            cancellationToken);

        if (!added)
        {
            var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);
            if (message is null)
            {
                return Result.Failure(new Error(
                    "Message.NotFound",
                    $"Message with ID {request.MessageId} not found"));
            }

            if (message.IsDeleted)
            {
                return Result.Failure(new Error(
                    "Message.Deleted",
                    "Cannot mark deleted message as read"));
            }

            // Idempotent: receipt already exists.
            return Result.Success();
        }

        return Result.Success();
    }
}
