using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Messages;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Commands.RemoveReaction;

/// <summary>
/// Handler for removing a reaction from a message
/// </summary>
public sealed class RemoveReactionCommandHandler : ICommandHandler<RemoveReactionCommand>
{
    private readonly IMessageRepository _messageRepository;

    public RemoveReactionCommandHandler(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<Result> Handle(
        RemoveReactionCommand request,
        CancellationToken cancellationToken)
    {
        // Get message
        var messageId = MessageId.Create(request.MessageId);
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure(new Error(
                "Message.NotFound",
                $"Message with ID {request.MessageId} not found"));
        }

        // Remove reaction using domain method
        var result = message.RemoveReaction(request.UserId, request.Emoji);

        if (result.IsFailure)
        {
            return result;
        }

        // Update message
        await _messageRepository.UpdateAsync(message, cancellationToken);

        return Result.Success();
    }
}
