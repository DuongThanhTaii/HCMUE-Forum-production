using Microsoft.EntityFrameworkCore;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Career.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of company repository for Career module
/// </summary>
public sealed class CompanyRepository : ICompanyRepository
{
    private readonly ApplicationDbContext _context;

    public CompanyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default)
    {
        await _context.Companies.AddAsync(company, cancellationToken);
    }

    public async Task<Company?> GetByIdAsync(CompanyId id, CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Company?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Name == name, cancellationToken);
    }

    public async Task<bool> IsNameUniqueAsync(string name, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Companies
            .AnyAsync(c => c.Name == name, cancellationToken);
        return !exists;
    }

    public Task UpdateAsync(Company company, CancellationToken cancellationToken = default)
    {
        _context.Companies.Update(company);
        return Task.CompletedTask;
    }

    public async Task<List<Company>> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Company>> GetByStatusAsync(CompanyStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .Where(c => c.Status == status)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Company>> GetByRegisteredByAsync(Guid registeredBy, CancellationToken cancellationToken = default)
    {
        return await _context.Companies
            .Where(c => c.RegisteredBy == registeredBy)
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
