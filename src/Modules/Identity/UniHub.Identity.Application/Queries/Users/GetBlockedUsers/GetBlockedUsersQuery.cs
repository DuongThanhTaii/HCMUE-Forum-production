using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Queries.Users.GetBlockedUsers;

public sealed record BlockedUserResponse(Guid UserId, DateTime BlockedAt);

public sealed record GetBlockedUsersQuery(Guid UserId) : IQuery<IReadOnlyList<BlockedUserResponse>>;
