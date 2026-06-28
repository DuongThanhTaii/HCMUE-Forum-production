using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Users;

public sealed class UserRole : Entity<Guid>
{
    public UserId UserId { get; private set; }
    public RoleId RoleId { get; private set; }
    public DateTime AssignedAt { get; private set; }

    private UserRole()
    {
        // EF Core constructor
        UserId = null!;
        RoleId = null!;
    }

    internal UserRole(UserId userId, RoleId roleId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RoleId = roleId;
        AssignedAt = DateTime.UtcNow;
    }
}