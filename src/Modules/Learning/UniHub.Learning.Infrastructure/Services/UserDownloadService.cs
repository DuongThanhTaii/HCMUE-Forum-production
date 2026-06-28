using Microsoft.EntityFrameworkCore;
using UniHub.Learning.Application.Abstractions;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Learning.Infrastructure.Services;

/// <summary>
/// Service to track user document downloads using a tracking table.
/// Ensures one download record per user per document.
/// </summary>
internal sealed class UserDownloadService : IUserDownloadService
{
    private readonly ApplicationDbContext _context;

    public UserDownloadService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasUserDownloadedDocumentAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT COUNT(1) AS ""Value""
            FROM user_document_downloads
            WHERE user_id = {0} AND document_id = {1}";

        var count = await _context.Database
            .SqlQueryRaw<int>(sql, userId, documentId)
            .FirstOrDefaultAsync(cancellationToken);

        return count > 0;
    }

    public async Task RecordUserDownloadAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        // Check if already downloaded (for idempotency)
        var hasDownloaded = await HasUserDownloadedDocumentAsync(userId, documentId, cancellationToken);
        if (hasDownloaded)
        {
            return; // Already downloaded, skip
        }

        var sql = @"
            INSERT INTO user_document_downloads (user_id, document_id, downloaded_at)
            VALUES ({0}, {1}, {2})
            ON CONFLICT (user_id, document_id) DO NOTHING";

        await _context.Database.ExecuteSqlRawAsync(
            sql,
            [userId, documentId, DateTime.UtcNow],
            cancellationToken);
    }
}
