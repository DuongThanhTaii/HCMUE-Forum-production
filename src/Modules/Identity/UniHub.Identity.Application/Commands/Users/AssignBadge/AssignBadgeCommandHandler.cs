using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Users.AssignBadge;

/// <summary>
/// Handler for assigning an official badge to a user
/// </summary>
public sealed class AssignBadgeCommandHandler : ICommandHandler<AssignBadgeCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionCache _permissionCache;

    public AssignBadgeCommandHandler(
        IUserRepository userRepository,
        IPermissionCache permissionCache)
    {
        _userRepository = userRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(AssignBadgeCommand request, CancellationToken cancellationToken)
    {
        // Get user
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        // Create badge
        var badgeResult = OfficialBadge.Create(
            request.BadgeType,
            request.BadgeName,
            request.VerifiedBy,
            request.Description);

        if (badgeResult.IsFailure)
        {
            return Result.Failure(badgeResult.Error);
        }

        // Assign badge to user
        var assignResult = user.SetOfficialBadge(badgeResult.Value);
        if (assignResult.IsFailure)
        {
            return assignResult;
        }

        // Save user
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate permission cache (badge might affect permissions)
        await _permissionCache.InvalidateUserPermissionsAsync(request.UserId, cancellationToken);

        return Result.Success();
    }
}
