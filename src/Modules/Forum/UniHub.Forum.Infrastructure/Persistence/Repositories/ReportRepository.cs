using Microsoft.EntityFrameworkCore;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Reports;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Forum.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of report repository for Forum module
/// </summary>
public sealed class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _context;

    public ReportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Report?> GetByIdAsync(ReportId id, CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Report?> GetByReporterAndItemAsync(
        Guid reporterId,
        Guid reportedItemId,
        ReportedItemType reportedItemType,
        CancellationToken cancellationToken = default)
    {
        return await _context.Reports
            .FirstOrDefaultAsync(r =>
                r.ReporterId == reporterId &&
                r.ReportedItemId == reportedItemId &&
                r.ReportedItemType == reportedItemType,
                cancellationToken);
    }

    public async Task<(IReadOnlyList<Report> Reports, int TotalCount)> GetReportsAsync(
        int pageNumber,
        int pageSize,
        ReportStatus? status = null,
        ReportResolutionDecision? resolutionDecision = null,
        IReadOnlyList<Guid>? categoryIds = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Reports.AsQueryable();

        // Join with Posts and Comments to filter by category if needed
        if (categoryIds != null && categoryIds.Count > 0)
        {
            query = query.Where(r =>
                (r.ReportedItemType == ReportedItemType.Post && _context.Posts.Any(p => p.Id == new PostId(r.ReportedItemId) && p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value))) ||
                (r.ReportedItemType == ReportedItemType.Comment && _context.Comments.Any(c => c.Id == new CommentId(r.ReportedItemId) && _context.Posts.Any(p => p.Id == c.PostId && p.CategoryId != null && categoryIds.Contains(p.CategoryId.Value)))));
        }

        // Filter by status if provided
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (resolutionDecision.HasValue)
        {
            query = query.Where(r => r.ResolutionDecision == resolutionDecision.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (reports.AsReadOnly(), totalCount);
    }

    public async Task AddAsync(Report report, CancellationToken cancellationToken = default)
    {
        await _context.Reports.AddAsync(report, cancellationToken);
    }

    public Task UpdateAsync(Report report, CancellationToken cancellationToken = default)
    {
        _context.Reports.Update(report);
        return Task.CompletedTask;
    }
}
