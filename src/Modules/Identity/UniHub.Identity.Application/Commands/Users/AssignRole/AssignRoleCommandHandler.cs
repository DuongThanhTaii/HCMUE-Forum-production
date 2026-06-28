using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Errors;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Users.AssignRole;

/// <summary>
/// Handler for assigning a role to a user
/// </summary>
public sealed class AssignRoleCommandHandler : ICommandHandler<AssignRoleCommand>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionCache _permissionCache;

    public AssignRoleCommandHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionCache permissionCache)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionCache = permissionCache;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
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

        // Assign role to user
        var assignResult = user.AssignRole(roleId);
        if (assignResult.IsFailure)
        {
            return assignResult;
        }

        // Save user
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Invalidate permission cache
        await _permissionCache.InvalidateUserPermissionsAsync(request.UserId, cancellationToken);

        return Result.Success();
    }
}
