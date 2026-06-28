using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Application.Abstractions;

public interface IUserGroupRepository
{
    Task<UserGroup?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<UserGroup?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<List<UserGroup>> GetByMemberAsync(UserId userId, CancellationToken cancellationToken = default);
    Task<List<UserGroup>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(UserGroup group, CancellationToken cancellationToken = default);
    Task UpdateAsync(UserGroup group, CancellationToken cancellationToken = default);
    Task DeleteAsync(UserGroup group, CancellationToken cancellationToken = default);
}
