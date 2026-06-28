using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Authorization;

public sealed class UserGroup : AggregateRoot<Guid>
{
    private readonly List<UserGroupMember> _members = new();

    public string Name { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyCollection<UserGroupMember> Members => _members.AsReadOnly();

    private UserGroup()
    {
        Name = string.Empty;
    }

    private UserGroup(string name, string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<UserGroup> Create(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<UserGroup>(new Error("UserGroup.Name.Empty", "Group name cannot be empty."));
        }

        var trimmedName = name.Trim();
        if (trimmedName.Length > 120)
        {
            return Result.Failure<UserGroup>(new Error("UserGroup.Name.TooLong", "Group name cannot exceed 120 characters."));
        }

        if (description?.Length > 500)
        {
            return Result.Failure<UserGroup>(new Error("UserGroup.Description.TooLong", "Group description cannot exceed 500 characters."));
        }

        return Result.Success(new UserGroup(trimmedName, description?.Trim()));
    }

    public Result Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure(new Error("UserGroup.Name.Empty", "Group name cannot be empty."));
        }

        var trimmedName = newName.Trim();
        if (trimmedName.Length > 120)
        {
            return Result.Failure(new Error("UserGroup.Name.TooLong", "Group name cannot exceed 120 characters."));
        }

        Name = trimmedName;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result UpdateDescription(string? description)
    {
        if (description?.Length > 500)
        {
            return Result.Failure(new Error("UserGroup.Description.TooLong", "Group description cannot exceed 500 characters."));
        }

        Description = description?.Trim();
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AddMember(UserId userId)
    {
        if (!IsActive)
        {
            return Result.Failure(new Error("UserGroup.Inactive", "Cannot add member to inactive group."));
        }

        if (_members.Any(member => member.UserId == userId))
        {
            return Result.Failure(new Error("UserGroup.Member.Exists", "User is already a member of the group."));
        }

        _members.Add(new UserGroupMember(Id, userId));
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result RemoveMember(UserId userId)
    {
        var member = _members.FirstOrDefault(item => item.UserId == userId);
        if (member is null)
        {
            return Result.Failure(new Error("UserGroup.Member.NotFound", "User is not a member of the group."));
        }

        _members.Remove(member);
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
