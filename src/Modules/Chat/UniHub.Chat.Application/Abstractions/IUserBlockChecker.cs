namespace UniHub.Chat.Application.Abstractions;

public interface IUserBlockChecker
{
    Task<bool> IsBlockedEitherWayAsync(Guid userA, Guid userB, CancellationToken cancellationToken = default);

    Task<IReadOnlySet<Guid>> GetBlockedUserIdsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
