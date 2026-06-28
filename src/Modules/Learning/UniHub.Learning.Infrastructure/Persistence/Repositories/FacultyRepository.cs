using Microsoft.EntityFrameworkCore;
using UniHub.Infrastructure.Persistence;
using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Faculties;

namespace UniHub.Learning.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of IFacultyRepository
/// </summary>
internal sealed class FacultyRepository : IFacultyRepository
{
    private readonly ApplicationDbContext _context;

    public FacultyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(Faculty faculty, CancellationToken cancellationToken = default)
    {
        _context.Faculties.Add(faculty);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Faculty faculty, CancellationToken cancellationToken = default)
    {
        _context.Faculties.Update(faculty);
        return Task.CompletedTask;
    }

    public async Task<Faculty?> GetByIdAsync(FacultyId id, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Faculty>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Faculties
            .AsNoTracking()
            .OrderBy(f => f.Name.Value)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties
            .AsNoTracking()
            .AnyAsync(f => f.Code.Value == code, cancellationToken);
    }

    public async Task<bool> ExistsAsync(FacultyId id, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties
            .AsNoTracking()
            .AnyAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Faculty?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Faculties
            .FirstOrDefaultAsync(f => f.Code.Value == code, cancellationToken);
    }
}
