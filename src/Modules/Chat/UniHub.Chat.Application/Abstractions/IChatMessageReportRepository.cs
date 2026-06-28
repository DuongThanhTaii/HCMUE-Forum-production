using UniHub.Chat.Domain.Safety;

namespace UniHub.Chat.Application.Abstractions;

public interface IChatMessageReportRepository
{
    Task<bool> ExistsAsync(
        Guid reporterId,
        Guid messageId,
        CancellationToken cancellationToken = default);

    Task AddAsync(ChatMessageReport report, CancellationToken cancellationToken = default);
}
