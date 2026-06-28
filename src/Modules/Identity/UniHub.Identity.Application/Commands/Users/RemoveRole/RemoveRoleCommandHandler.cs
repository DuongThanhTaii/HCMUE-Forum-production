using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Users.RemoveRole;

/// <summary>
/// Handler for removing a role from a user
/// </summary>
public sealed class RemoveRoleCommandHandler : ICommandHandler<RemoveRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionCache _permissionCache;

    public RemoveRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionCache permissionCache)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        // Get user
        var userId = new UserId(request.UserId);
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(UserErrors.NotFound);
        }

        // Get role
        var roleId = new RoleId(request.RoleId);
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(new Error("Role.NotFound", "Role not found"));
        }

        // Remove role from user
        var removeResult = user.RemoveRole(roleId);
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
