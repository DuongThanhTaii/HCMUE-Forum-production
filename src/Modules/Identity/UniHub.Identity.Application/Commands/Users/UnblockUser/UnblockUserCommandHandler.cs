using UniHub.Identity.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Users.UnblockUser;

public sealed class UnblockUserCommandHandler : ICommandHandler<UnblockUserCommand>
{
    private readonly IUserBlockRepository _userBlockRepository;

    public UnblockUserCommandHandler(IUserBlockRepository userBlockRepository)
    {
        _userBlockRepository = userBlockRepository;
    }

    public async Task<Result> Handle(UnblockUserCommand request, CancellationToken cancellationToken)
    {
        await _userBlockRepository.RemoveAsync(
            request.BlockerUserId,
            request.BlockedUserId,
            cancellationToken);

        return Result.Success();
    }
}
