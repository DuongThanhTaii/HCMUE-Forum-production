using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Users.BlockUser;

public sealed class BlockUserCommandHandler : ICommandHandler<BlockUserCommand>
{
    private readonly IUserBlockRepository _userBlockRepository;

    public BlockUserCommandHandler(IUserBlockRepository userBlockRepository)
    {
        _userBlockRepository = userBlockRepository;
    }

    public async Task<Result> Handle(BlockUserCommand request, CancellationToken cancellationToken)
    {
        if (await _userBlockRepository.ExistsAsync(
                request.BlockerUserId,
                request.BlockedUserId,
                cancellationToken))
        {
            return Result.Success();
        }

        var blockResult = UserBlock.Create(request.BlockerUserId, request.BlockedUserId);
        if (blockResult.IsFailure)
        {
            return Result.Failure(blockResult.Error);
        }

        await _userBlockRepository.AddAsync(blockResult.Value, cancellationToken);
        return Result.Success();
    }
}
