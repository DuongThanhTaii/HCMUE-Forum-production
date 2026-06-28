using UniHub.SharedKernel.CQRS;

namespace UniHub.Chat.Application.Queries.GetMessageReadReceipts;

/// <summary>
/// Query to get read receipts for a message
/// </summary>
public sealed record GetMessageReadReceiptsQuery(
    Guid MessageId) : IQuery<List<ReadReceiptResponse>>;

/// <summary>
/// Read receipt response
/// </summary>
public sealed record ReadReceiptResponse(
    Guid UserId,
    DateTime ReadAt);
