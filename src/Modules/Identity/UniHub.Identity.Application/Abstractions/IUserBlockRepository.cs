using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Application.Abstractions;

public interface IUserBlockRepository
{
    Task<bool> ExistsAsync(Guid blockerUserId, Guid blockedUserId, CancellationToken cancellationToken = default);

    Task<bool> IsBlockedEitherWayAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserBlock>> GetBlockedByUserAsync(
        Guid blockerUserId,
        CancellationToken cancellationToken = default);

    Task AddAsync(UserBlock block, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid blockerUserId, Guid blockedUserId, CancellationToken cancellationToken = default);
}
