using Microsoft.EntityFrameworkCore;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.JobPostings;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Career.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of job posting repository for Career module
/// </summary>
public sealed class JobPostingRepository : IJobPostingRepository
{
    private readonly ApplicationDbContext _context;

    public JobPostingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(JobPosting jobPosting, CancellationToken cancellationToken = default)
    {
        await _context.JobPostings.AddAsync(jobPosting, cancellationToken);
    }

    public Task UpdateAsync(JobPosting jobPosting, CancellationToken cancellationToken = default)
    {
        _context.JobPostings.Update(jobPosting);
        return Task.CompletedTask;
    }

    public async Task<JobPosting?> GetByIdAsync(JobPostingId id, CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<(List<JobPosting> JobPostings, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        Guid? companyId = null,
        JobType? jobType = null,
        ExperienceLevel? experienceLevel = null,
        JobPostingStatus? status = null,
        string? city = null,
        bool? isRemote = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.JobPostings.AsQueryable();

        // Apply filters
        if (companyId.HasValue)
            query = query.Where(j => j.CompanyId == companyId.Value);

        if (jobType.HasValue)
            query = query.Where(j => j.JobType == jobType.Value);

        if (experienceLevel.HasValue)
            query = query.Where(j => j.ExperienceLevel == experienceLevel.Value);

        if (status.HasValue)
            query = query.Where(j => j.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(j => j.Location.City.Contains(city));

        if (isRemote.HasValue)
            query = query.Where(j => j.Location.IsRemote == isRemote.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(j =>
                j.Title.Contains(searchTerm) ||
                j.Description.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var jobPostings = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (jobPostings, totalCount);
    }

    public async Task<List<JobPosting>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .Where(j => j.CompanyId == companyId)
            .OrderByDescending(j => j.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<JobPosting>> GetPublishedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .Where(j => j.Status == JobPostingStatus.Published)
            .OrderByDescending(j => j.PublishedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(JobPostingId id, CancellationToken cancellationToken = default)
    {
        var jobPosting = await _context.JobPostings
            .FirstOrDefaultAsync(j => j.Id == id, cancellationToken);
        
        if (jobPosting != null)
        {
            _context.JobPostings.Remove(jobPosting);
        }
    }

    public async Task<bool> ExistsAsync(JobPostingId id, CancellationToken cancellationToken = default)
    {
        return await _context.JobPostings
            .AnyAsync(j => j.Id == id, cancellationToken);
    }

    public async Task<(List<JobPosting> JobPostings, int TotalCount)> SearchAsync(
        string? keywords = null,
        Guid? companyId = null,
        JobType? jobType = null,
        ExperienceLevel? experienceLevel = null,
        JobPostingStatus? status = null,
        string? city = null,
        bool? isRemote = null,
        decimal? minSalary = null,
        decimal? maxSalary = null,
        string? currency = null,
        List<string>? requiredSkills = null,
        List<string>? tags = null,
        DateTime? postedAfter = null,
        DateTime? postedBefore = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.JobPostings.AsQueryable();

        // Apply keyword search
        if (!string.IsNullOrWhiteSpace(keywords))
        {
            query = query.Where(j =>
                j.Title.Contains(keywords) ||
                j.Description.Contains(keywords));
        }

        // Apply filters
        if (companyId.HasValue)
            query = query.Where(j => j.CompanyId == companyId.Value);

        if (jobType.HasValue)
            query = query.Where(j => j.JobType == jobType.Value);

        if (experienceLevel.HasValue)
            query = query.Where(j => j.ExperienceLevel == experienceLevel.Value);

        if (status.HasValue)
            query = query.Where(j => j.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(city))
            query = query.Where(j => j.Location.City.Contains(city));

        if (isRemote.HasValue)
            query = query.Where(j => j.Location.IsRemote == isRemote.Value);

        // Salary range filters
        if (minSalary.HasValue || maxSalary.HasValue || !string.IsNullOrWhiteSpace(currency))
        {
            // Note: Salary is a Value Object, filtering may need adjustments based on implementation
            if (minSalary.HasValue)
                query = query.Where(j => j.Salary!.MinAmount >= minSalary.Value);

            if (maxSalary.HasValue)
                query = query.Where(j => j.Salary!.MaxAmount <= maxSalary.Value);

            if (!string.IsNullOrWhiteSpace(currency))
                query = query.Where(j => j.Salary!.Currency == currency);
        }

        // Date range filters
        if (postedAfter.HasValue)
            query = query.Where(j => j.PublishedAt.HasValue && j.PublishedAt.Value >= postedAfter.Value);

        if (postedBefore.HasValue)
            query = query.Where(j => j.PublishedAt.HasValue && j.PublishedAt.Value <= postedBefore.Value);

        // Note: Tags are stored as JSONB, filtering would require specific EF Core JSONB queries
        // For now, we'll load results and filter in memory if needed, or implement server-side JSON queries

        var totalCount = await query.CountAsync(cancellationToken);

        var jobPostings = await query
            .OrderByDescending(j => j.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (jobPostings, totalCount);
    }
}
