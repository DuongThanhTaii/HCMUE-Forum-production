using Microsoft.EntityFrameworkCore;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Permissions;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of permission repository
/// </summary>
public sealed class PermissionRepository : IPermissionRepository
{
    private readonly ApplicationDbContext _context;

    public PermissionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Permission?> GetByIdAsync(PermissionId id, CancellationToken cancellationToken = default)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return permissions.AsReadOnly();
    }

    public async Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToLowerInvariant();

        return await _context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Code == normalizedCode, cancellationToken);
    }
}
