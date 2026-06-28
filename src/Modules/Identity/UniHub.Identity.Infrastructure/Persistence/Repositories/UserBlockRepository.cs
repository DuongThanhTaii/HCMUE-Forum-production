using Microsoft.EntityFrameworkCore;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserBlockRepository : IUserBlockRepository
{
    private readonly ApplicationDbContext _context;

    public UserBlockRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(
        Guid blockerUserId,
        Guid blockedUserId,
        CancellationToken cancellationToken = default) =>
        _context.UserBlocks.AnyAsync(
            b => b.BlockerUserId == blockerUserId && b.BlockedUserId == blockedUserId,
            cancellationToken);

    public Task<bool> IsBlockedEitherWayAsync(
        Guid userA,
        Guid userB,
        CancellationToken cancellationToken = default) =>
        _context.UserBlocks.AnyAsync(
            b =>
                (b.BlockerUserId == userA && b.BlockedUserId == userB) ||
                (b.BlockerUserId == userB && b.BlockedUserId == userA),
            cancellationToken);

    public async Task<IReadOnlyList<UserBlock>> GetBlockedByUserAsync(
        Guid blockerUserId,
        CancellationToken cancellationToken = default)
    {
        var items = await _context.UserBlocks
            .AsNoTracking()
            .Where(b => b.BlockerUserId == blockerUserId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return items.AsReadOnly();
    }

    public async Task AddAsync(UserBlock block, CancellationToken cancellationToken = default)
    {
        await _context.UserBlocks.AddAsync(block, cancellationToken);
    }

    public async Task RemoveAsync(
        Guid blockerUserId,
        Guid blockedUserId,
        CancellationToken cancellationToken = default)
    {
        var entity = await _context.UserBlocks.FirstOrDefaultAsync(
            b => b.BlockerUserId == blockerUserId && b.BlockedUserId == blockedUserId,
            cancellationToken);

        if (entity is not null)
        {
            _context.UserBlocks.Remove(entity);
        }
    }
}
