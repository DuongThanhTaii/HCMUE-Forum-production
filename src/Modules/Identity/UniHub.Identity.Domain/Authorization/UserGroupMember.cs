using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Domain;

namespace UniHub.Identity.Domain.Authorization;

public sealed class UserGroupMember : Entity<Guid>
{
    public Guid GroupId { get; private set; }
    public UserId UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private UserGroupMember()
    {
        UserId = null!;
    }

    internal UserGroupMember(Guid groupId, UserId userId)
    {
        Id = Guid.NewGuid();
        GroupId = groupId;
        UserId = userId;
        JoinedAt = DateTime.UtcNow;
    }
}
