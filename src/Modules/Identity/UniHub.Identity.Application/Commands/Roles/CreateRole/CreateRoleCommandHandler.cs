using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Roles;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Roles.CreateRole;

/// <summary>
/// Handler for creating a new role
/// </summary>
public sealed class CreateRoleCommandHandler : ICommandHandler<CreateRoleCommand, Guid>
{
    private readonly IRoleRepository _roleRepository;

    public CreateRoleCommandHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // Check if role name already exists
        var existingRole = await _roleRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingRole is not null)
        {
            return Result.Failure<Guid>(RoleErrors.RoleAlreadyExists);
        }

        // Create role
        var roleResult = Role.Create(request.Name, request.Description);
        if (roleResult.IsFailure)
        {
            return Result.Failure<Guid>(roleResult.Error);
        }

        var role = roleResult.Value;

        // Save role
        await _roleRepository.AddAsync(role, cancellationToken);

        return Result.Success(role.Id.Value);
    }
}
