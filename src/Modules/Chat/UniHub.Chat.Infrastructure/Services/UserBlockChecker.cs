using Microsoft.EntityFrameworkCore;
using UniHub.Chat.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Chat.Infrastructure.Services;

public sealed class UserBlockChecker : IUserBlockChecker
{
    private readonly ApplicationDbContext _context;

    public UserBlockChecker(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<bool> IsBlockedEitherWayAsync(
        Guid userA,
        Guid userB,
        CancellationToken cancellationToken = default) =>
        _context.Set<UserBlock>().AnyAsync(
            b =>
                (b.BlockerUserId == userA && b.BlockedUserId == userB) ||
                (b.BlockerUserId == userB && b.BlockedUserId == userA),
            cancellationToken);

    public async Task<IReadOnlySet<Guid>> GetBlockedUserIdsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _context.Set<UserBlock>()
            .AsNoTracking()
            .Where(b => b.BlockerUserId == userId || b.BlockedUserId == userId)
            .ToListAsync(cancellationToken);

        var set = new HashSet<Guid>();
        foreach (var row in rows)
        {
            if (row.BlockerUserId == userId)
            {
                set.Add(row.BlockedUserId);
            }
            else
            {
                set.Add(row.BlockerUserId);
            }
        }

        return set;
    }
}
