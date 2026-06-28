using Microsoft.EntityFrameworkCore;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Career.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of recruiter repository for Career module
/// </summary>
public sealed class RecruiterRepository : IRecruiterRepository
{
    private readonly ApplicationDbContext _context;

    public RecruiterRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Recruiter recruiter, CancellationToken cancellationToken = default)
    {
        await _context.Recruiters.AddAsync(recruiter, cancellationToken);
    }

    public Task UpdateAsync(Recruiter recruiter, CancellationToken cancellationToken = default)
    {
        _context.Recruiters.Update(recruiter);
        return Task.CompletedTask;
    }

    public async Task<Recruiter?> GetByIdAsync(RecruiterId id, CancellationToken cancellationToken = default)
    {
        return await _context.Recruiters
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Recruiter?> GetByUserAndCompanyAsync(Guid userId, CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Recruiters
            .FirstOrDefaultAsync(r => r.UserId == userId && r.CompanyId == companyId, cancellationToken);
    }

    public async Task<List<Recruiter>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Recruiters
            .Where(r => r.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Recruiter>> GetByCompanyAsync(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Recruiters
            .Where(r => r.CompanyId == companyId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Recruiter>> GetActiveByCompanyAsync(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Recruiters
            .Where(r => r.CompanyId == companyId && r.Status == RecruiterStatus.Active)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid userId, CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Recruiters
            .AnyAsync(r => r.UserId == userId && r.CompanyId == companyId, cancellationToken);
    }

    public async Task<int> GetActiveCountAsync(CompanyId companyId, CancellationToken cancellationToken = default)
    {
        return await _context.Recruiters
            .CountAsync(r => r.CompanyId == companyId && r.Status == RecruiterStatus.Active, cancellationToken);
    }
}
