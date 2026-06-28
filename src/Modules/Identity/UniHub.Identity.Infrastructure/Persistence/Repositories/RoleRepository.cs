using Microsoft.EntityFrameworkCore;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Roles;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of role repository
/// </summary>
public sealed class RoleRepository : IRoleRepository
{
    private readonly ApplicationDbContext _context;

    public RoleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(RoleId roleId, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Role>> GetByIdsAsync(IEnumerable<RoleId> roleIds, CancellationToken cancellationToken = default)
    {
        var idList = roleIds.Distinct().ToList();
        if (idList.Count == 0)
        {
            return new List<Role>();
        }

        return await _context.Roles
            .AsNoTracking()
            .Where(r => idList.Contains(r.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
    }

    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(role);
        if (entry.State == EntityState.Detached)
        {
            _context.Roles.Update(role);
            return Task.CompletedTask;
        }

        // Detect changes so that new RolePermissions added to the list are tracked.
        _context.ChangeTracker.DetectChanges();

        // EF Core incorrectly marks newly instantiated RolePermissions as Modified
        // because their Id (Guid) is generated in the constructor (non-default value).
        // Since RolePermissions are immutable, any 'Modified' permission is actually a new one.
        foreach (var p in role.Permissions)
        {
            var pEntry = _context.Entry(p);
            if (pEntry.State == EntityState.Modified)
            {
                pEntry.State = EntityState.Added;
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Remove(role);
        return Task.CompletedTask;
    }
}
