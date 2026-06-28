using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

public sealed class UserGroupRepository : IUserGroupRepository
{
    private static readonly List<UserGroup> Groups = new();
    private static readonly object LockObj = new();
    private static bool _seeded;

    public UserGroupRepository()
    {
        EnsureSeeded();
    }

    public Task<UserGroup?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            return Task.FromResult(Groups.FirstOrDefault(item => item.Id == groupId));
        }
    }

    public Task<UserGroup?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            return Task.FromResult(Groups.FirstOrDefault(item =>
                item.Name.Equals(name, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public Task<List<UserGroup>> GetByMemberAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            var result = Groups
                .Where(group => group.Members.Any(member => member.UserId == userId))
                .ToList();

            return Task.FromResult(result);
        }
    }

    public Task<List<UserGroup>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            return Task.FromResult(Groups.ToList());
        }
    }

    public Task AddAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            Groups.Add(group);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            Groups.Remove(group);
        }

        return Task.CompletedTask;
    }

    private static void EnsureSeeded()
    {
        lock (LockObj)
        {
            if (_seeded || Groups.Count > 0)
            {
                _seeded = true;
                return;
            }

            var groupDefinitions = new[]
            {
                ("Forum Moderators", "Nhóm kiểm duyệt nội dung diễn đàn và học tập."),
                ("Learning Moderators", "Nhóm phụ trách duyệt tài liệu học tập theo môn."),
                ("Career Moderators", "Nhóm phụ trách kiểm duyệt bài tuyển dụng."),
            };

            foreach (var (name, description) in groupDefinitions)
            {
                var groupResult = UserGroup.Create(name, description);
                if (groupResult.IsFailure)
                {
                    continue;
                }

                var group = groupResult.Value;
                _ = group.AddMember(UserId.Create(Guid.Parse("00000000-0000-0000-0000-000000000001")));
                Groups.Add(group);
            }

            _seeded = true;
        }
    }
}
