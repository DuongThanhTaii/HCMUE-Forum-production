using Microsoft.EntityFrameworkCore;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using UniHub.Infrastructure.Persistence;
using DomainApplication = UniHub.Career.Domain.Applications.Application;
using DomainApplicationId = UniHub.Career.Domain.Applications.ApplicationId;

namespace UniHub.Career.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of application repository for Career module
/// </summary>
public sealed class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;

    public ApplicationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DomainApplication application, CancellationToken cancellationToken = default)
    {
        await _context.Applications.AddAsync(application, cancellationToken);
    }

    public Task UpdateAsync(DomainApplication application, CancellationToken cancellationToken = default)
    {
        _context.Applications.Update(application);
        return Task.CompletedTask;
    }

    public async Task<DomainApplication?> GetByIdAsync(DomainApplicationId id, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<DomainApplication?> GetByJobAndApplicantAsync(
        JobPostingId jobPostingId,
        Guid applicantId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .FirstOrDefaultAsync(a =>
                a.JobPostingId == jobPostingId &&
                a.ApplicantId == applicantId,
                cancellationToken);
    }

    public async Task<(List<DomainApplication> Applications, int TotalCount)> GetByJobPostingAsync(
        JobPostingId jobPostingId,
        ApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Applications
            .Where(a => a.JobPostingId == jobPostingId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (applications, totalCount);
    }

    public async Task<(List<DomainApplication> Applications, int TotalCount)> GetByApplicantAsync(
        Guid applicantId,
        ApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Applications
            .Where(a => a.ApplicantId == applicantId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (applications, totalCount);
    }

    public async Task<(List<DomainApplication> Applications, int TotalCount)> GetByCompanyAsync(
        Guid companyId,
        ApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Applications
            .Join(_context.JobPostings,
                app => app.JobPostingId,
                job => job.Id,
                (app, job) => new { Application = app, Job = job })
            .Where(x => x.Job.CompanyId == companyId)
            .Select(x => x.Application);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var applications = await query
            .OrderByDescending(a => a.SubmittedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return (applications, totalCount);
    }

    public async Task<bool> ExistsAsync(DomainApplicationId id, CancellationToken cancellationToken = default)
    {
        return await _context.Applications
            .AnyAsync(a => a.Id == id, cancellationToken);
    }
}
