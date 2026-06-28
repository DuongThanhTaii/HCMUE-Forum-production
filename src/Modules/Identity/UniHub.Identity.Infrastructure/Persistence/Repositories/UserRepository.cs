using Microsoft.EntityFrameworkCore;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of user repository
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> SearchAsync(string searchTerm, int take, CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);
        var term = searchTerm.Trim();
        if (term.Length == 0)
        {
            return Array.Empty<User>();
        }

        var pattern = $"%{EscapeForLike(term)}%";

        // ILike on u.Email or Email.Value does not compose safely with the Email value-converter
        // (translation or SQL literal generation). Use parameterized SQL on real columns instead.
        return await _context.Users
            .FromSqlInterpolated(
                $"""
                SELECT * FROM identity.users
                WHERE first_name ILIKE {pattern} ESCAPE '\'
                   OR last_name ILIKE {pattern} ESCAPE '\'
                   OR email ILIKE {pattern} ESCAPE '\'
                ORDER BY last_name, first_name
                LIMIT {take}
                """)
            .Include(u => u.Roles)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private static string EscapeForLike(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }

    public async Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);
        return !exists;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var entry = _context.Entry(user);
        if (entry.State == EntityState.Detached)
        {
            _context.Users.Attach(user);
            entry.State = EntityState.Modified;
        }

        foreach (var userRole in user.Roles)
        {
            var roleEntry = _context.Entry(userRole);
            if (roleEntry.State == EntityState.Detached)
            {
                _context.UserRoles.Add(userRole);
            }
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }
}
