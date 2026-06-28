using Microsoft.EntityFrameworkCore;
using UniHub.Career.Application.Abstractions;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Career.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of saved job repository for Career module
/// Note: SavedJob is not a domain entity, it's a simple many-to-many relationship
/// </summary>
public sealed class SavedJobRepository : ISavedJobRepository
{
    private readonly ApplicationDbContext _context;

    public SavedJobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SaveJobAsync(Guid userId, Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        // Check if already saved
        var exists = await IsSavedAsync(userId, jobPostingId, cancellationToken);
        if (exists)
            return;

        // For now, we'll use raw SQL since SavedJob is not a domain entity
        // TODO: Consider creating a proper domain entity or EF Core configuration for this relationship
        await _context.Database.ExecuteSqlRawAsync(
            "INSERT INTO career.saved_jobs (user_id, job_posting_id, saved_at) VALUES ({0}, {1}, {2})",
            userId, jobPostingId, DateTime.UtcNow);
    }

    public async Task UnsaveJobAsync(Guid userId, Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlRawAsync(
            "DELETE FROM career.saved_jobs WHERE user_id = {0} AND job_posting_id = {1}",
            userId, jobPostingId);
    }

    public async Task<List<SavedJob>> GetSavedJobsByUserAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Use FromSqlRaw to query the saved_jobs table
        var savedJobs = await _context.Database
            .SqlQuery<SavedJob>($@"
                SELECT user_id as UserId, job_posting_id as JobPostingId, saved_at as SavedAt
                FROM career.saved_jobs
                WHERE user_id = {userId}
                ORDER BY saved_at DESC
                LIMIT {pageSize} OFFSET {(page - 1) * pageSize}")
            .ToListAsync(cancellationToken);

        return savedJobs;
    }

    public async Task<bool> IsSavedAsync(Guid userId, Guid jobPostingId, CancellationToken cancellationToken = default)
    {
        var count = await _context.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*)
                FROM career.saved_jobs
                WHERE user_id = {userId} AND job_posting_id = {jobPostingId}")
            .FirstOrDefaultAsync(cancellationToken);

        return count > 0;
    }

    public async Task<int> GetSavedCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Database
            .SqlQuery<int>($@"
                SELECT COUNT(*)
                FROM career.saved_jobs
                WHERE user_id = {userId}")
            .FirstOrDefaultAsync(cancellationToken);
    }
}
