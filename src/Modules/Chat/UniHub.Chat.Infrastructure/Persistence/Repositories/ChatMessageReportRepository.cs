using Microsoft.EntityFrameworkCore;
using UniHub.Chat.Application.Abstractions;
using UniHub.Chat.Domain.Safety;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Chat.Infrastructure.Persistence.Repositories;

public sealed class ChatMessageReportRepository : IChatMessageReportRepository
{
    private readonly ApplicationDbContext _context;

    public ChatMessageReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(
        Guid reporterId,
        Guid messageId,
        CancellationToken cancellationToken = default) =>
        _context.ChatMessageReports.AnyAsync(
            r => r.ReporterId == reporterId && r.MessageId == messageId,
            cancellationToken);

    public async Task AddAsync(ChatMessageReport report, CancellationToken cancellationToken = default)
    {
        await _context.ChatMessageReports.AddAsync(report, cancellationToken);
    }
}
