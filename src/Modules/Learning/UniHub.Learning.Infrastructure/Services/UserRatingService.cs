using Microsoft.EntityFrameworkCore;
using UniHub.Learning.Application.Abstractions;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Learning.Infrastructure.Services;

/// <summary>
/// Service to track user document ratings using a tracking table.
/// Ensures one rating per user per document constraint.
/// </summary>
internal sealed class UserRatingService : IUserRatingService
{
    private readonly ApplicationDbContext _context;

    public UserRatingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasUserRatedDocumentAsync(
        Guid userId, 
        Guid documentId, 
        CancellationToken cancellationToken = default)
    {
        var sql = @"
            SELECT COUNT(1) AS ""Value""
            FROM user_document_ratings
            WHERE user_id = {0} AND document_id = {1}";

        var count = await _context.Database
            .SqlQueryRaw<int>(sql, userId, documentId)
            .FirstOrDefaultAsync(cancellationToken);

        return count > 0;
    }

    public async Task RecordUserRatingAsync(
        Guid userId, 
        Guid documentId, 
        int rating, 
        CancellationToken cancellationToken = default)
    {
        // Check if already rated (for idempotency)
        var hasRated = await HasUserRatedDocumentAsync(userId, documentId, cancellationToken);
        if (hasRated)
        {
            return; // Already rated, skip
        }

        var sql = @"
            INSERT INTO user_document_ratings (user_id, document_id, rating, rated_at)
            VALUES ({0}, {1}, {2}, {3})
            ON CONFLICT (user_id, document_id) DO NOTHING";

        await _context.Database.ExecuteSqlRawAsync(
            sql, 
            [userId, documentId, rating, DateTime.UtcNow],
            cancellationToken);
    }
}
