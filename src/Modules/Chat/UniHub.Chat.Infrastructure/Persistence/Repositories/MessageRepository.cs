using Microsoft.EntityFrameworkCore;
using UniHub.Chat.Application.Abstractions;
using System.Text;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Chat.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of message repository for Chat module
/// </summary>
public sealed class MessageRepository : IMessageRepository
{
    private readonly ApplicationDbContext _context;

    public MessageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Message?> GetByIdAsync(MessageId id, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Message>> GetByConversationIdAsync(
        ConversationId conversationId,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var messages = await _context.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return messages.AsReadOnly();
    }

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        await _context.Messages.AddAsync(message, cancellationToken);
    }

    public Task UpdateAsync(Message message, CancellationToken cancellationToken = default)
    {
        // Tracked entities from GetByIdAsync: SaveChanges only. Update() on the aggregate
        // can corrupt owned collections (read receipts) and cause concurrency exceptions.
        if (_context.Entry(message).State == EntityState.Detached)
        {
            _context.Messages.Update(message);
        }

        return Task.CompletedTask;
    }

    public async Task<bool> TryAddReadReceiptAsync(
        MessageId messageId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Messages
            .AsNoTracking()
            .AnyAsync(
                m => m.Id == messageId && !m.IsDeleted,
                cancellationToken);

        if (!exists)
        {
            return false;
        }

        var readAt = DateTime.UtcNow;
        var rows = await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
            INSERT INTO chat.message_read_receipts (message_id, user_id, read_at)
            VALUES ({messageId.Value}, {userId}, {readAt})
            ON CONFLICT (message_id, user_id) DO NOTHING
            """,
            cancellationToken);

        return rows > 0;
    }

    public async Task<int> CountByConversationIdAsync(ConversationId conversationId, CancellationToken cancellationToken = default)
    {
        return await _context.Messages
            .CountAsync(m => m.ConversationId == conversationId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Message> Items, int TotalCount)> SearchByConversationIdAsync(
        ConversationId conversationId,
        string query,
        ConversationMessageSearchFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var pattern = $"%{EscapeLikePattern(query)}%";
        var baseQuery = _context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted);

        baseQuery = filter switch
        {
            ConversationMessageSearchFilter.Text => baseQuery.Where(m =>
                EF.Functions.ILike(m.Content, pattern)),
            ConversationMessageSearchFilter.Media => baseQuery.Where(m => m.Attachments.Any()),
            ConversationMessageSearchFilter.Links => baseQuery.Where(m =>
                EF.Functions.ILike(m.Content, "%http%") ||
                EF.Functions.ILike(m.Content, "%www.%")),
            _ => baseQuery.Where(m =>
                EF.Functions.ILike(m.Content, pattern) ||
                m.Attachments.Any() ||
                EF.Functions.ILike(m.Content, "%http%") ||
                EF.Functions.ILike(m.Content, "%www.%")),
        };

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.AsReadOnly(), totalCount);
    }

    private static string EscapeLikePattern(string value)
    {
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch is '%' or '_' or '\\')
            {
                sb.Append('\\');
            }

            sb.Append(ch);
        }

        return sb.ToString();
    }

    public async Task<(IReadOnlyList<ConversationAttachmentListItem> Items, int TotalCount)>
        ListAttachmentsByConversationIdAsync(
            ConversationId conversationId,
            ConversationAttachmentKind kind,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
    {
        var flatQuery = _context.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted && m.Attachments.Any())
            .SelectMany(
                m => m.Attachments,
                (m, a) => new
                {
                    MessageId = m.Id.Value,
                    m.SentAt,
                    a.FileName,
                    a.FileUrl,
                    a.MimeType,
                    a.ThumbnailUrl,
                    FileSizeBytes = a.FileSizeBytes,
                });

        flatQuery = kind switch
        {
            ConversationAttachmentKind.Image => flatQuery.Where(x =>
                EF.Functions.ILike(x.MimeType, "image/%") ||
                EF.Functions.ILike(x.FileName, "%.jpg") ||
                EF.Functions.ILike(x.FileName, "%.jpeg") ||
                EF.Functions.ILike(x.FileName, "%.png") ||
                EF.Functions.ILike(x.FileName, "%.gif") ||
                EF.Functions.ILike(x.FileName, "%.webp") ||
                EF.Functions.ILike(x.FileName, "%.bmp") ||
                EF.Functions.ILike(x.FileName, "%.svg")),
            ConversationAttachmentKind.Voice => flatQuery.Where(x =>
                EF.Functions.ILike(x.MimeType, "audio/%")),
            ConversationAttachmentKind.File => flatQuery.Where(x =>
                !EF.Functions.ILike(x.MimeType, "image/%") &&
                !EF.Functions.ILike(x.MimeType, "audio/%")),
            _ => flatQuery,
        };

        var totalCount = await flatQuery.CountAsync(cancellationToken);

        var rawItems = await flatQuery
            .OrderByDescending(x => x.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = rawItems
            .Select(x => new ConversationAttachmentListItem(
                x.MessageId,
                x.SentAt,
                x.FileName,
                x.FileUrl,
                x.MimeType,
                x.ThumbnailUrl,
                x.FileSizeBytes))
            .ToList();

        return (items.AsReadOnly(), totalCount);
    }

    public async Task<(IReadOnlyList<Message> Items, int TotalCount)> ListMessagesWithLinksAsync(
        ConversationId conversationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Messages
            .AsNoTracking()
            .Where(m =>
                m.ConversationId == conversationId &&
                !m.IsDeleted &&
                (EF.Functions.ILike(m.Content, "%http%") ||
                 EF.Functions.ILike(m.Content, "%www.%")));

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var items = await baseQuery
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items.AsReadOnly(), totalCount);
    }
}
