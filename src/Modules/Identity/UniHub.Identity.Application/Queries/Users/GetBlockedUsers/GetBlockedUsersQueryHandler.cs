using UniHub.Identity.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Queries.Users.GetBlockedUsers;

public sealed class GetBlockedUsersQueryHandler
    : IQueryHandler<GetBlockedUsersQuery, IReadOnlyList<BlockedUserResponse>>
{
    private readonly IUserBlockRepository _userBlockRepository;

    public GetBlockedUsersQueryHandler(IUserBlockRepository userBlockRepository)
    {
        _userBlockRepository = userBlockRepository;
    }

    public async Task<Result<IReadOnlyList<BlockedUserResponse>>> Handle(
        GetBlockedUsersQuery request,
        CancellationToken cancellationToken)
    {
        var blocks = await _userBlockRepository.GetBlockedByUserAsync(request.UserId, cancellationToken);

        var response = blocks
            .Select(b => new BlockedUserResponse(b.BlockedUserId, b.CreatedAt))
            .ToList();

        return Result.Success<IReadOnlyList<BlockedUserResponse>>(response);
    }
}
