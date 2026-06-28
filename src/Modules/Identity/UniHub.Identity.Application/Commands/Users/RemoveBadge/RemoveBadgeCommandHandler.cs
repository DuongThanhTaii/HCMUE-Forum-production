using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Users.RemoveBadge;

/// <summary>
/// Handler for removing an official badge from a user
/// </summary>
public sealed class RemoveBadgeCommandHandler : ICommandHandler<RemoveBadgeCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionCache _permissionCache;

    public RemoveBadgeCommandHandler(
        IUserRepository userRepository,
        IPermissionCache permissionCache)
    {
        _userRepository = userRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(RemoveBadgeCommand request, CancellationToken cancellationToken)
    {
        // Get user
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        // Remove badge from user
        var removeResult = user.RemoveOfficialBadge();
        if (removeResult.IsFailure)
        {
            return removeResult;
        }

        // Save user
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate permission cache
        await _permissionCache.InvalidateUserPermissionsAsync(request.UserId, cancellationToken);

        return Result.Success();
    }
}
