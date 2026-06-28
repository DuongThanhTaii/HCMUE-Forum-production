using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Messages;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Queries.GetMessageReadReceipts;

/// <summary>
/// Handler for getting message read receipts
/// </summary>
public sealed class GetMessageReadReceiptsQueryHandler 
    : IQueryHandler<GetMessageReadReceiptsQuery, List<ReadReceiptResponse>>
{
    private readonly IMessageRepository _messageRepository;

    public GetMessageReadReceiptsQueryHandler(IMessageRepository messageRepository)
    {
        _messageRepository = messageRepository;
    }

    public async Task<Result<List<ReadReceiptResponse>>> Handle(
        GetMessageReadReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        // Get message
        var messageId = MessageId.Create(request.MessageId);
        var message = await _messageRepository.GetByIdAsync(messageId, cancellationToken);

        if (message is null)
        {
            return Result.Failure<List<ReadReceiptResponse>>(new Error(
                "Message.NotFound",
                $"Message with ID {request.MessageId} not found"));
        }

        // Map read receipts to response
        var readReceipts = message.ReadReceipts
            .Select(r => new ReadReceiptResponse(r.UserId, r.ReadAt))
            .OrderBy(r => r.ReadAt)
            .ToList();

        return Result.Success(readReceipts);
    }
}
