using UniHub.Chat.Domain.Channels.Events;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Domain.Channels;

/// <summary>
/// Channel aggregate - đại diện cho một channel (public/private chat room)
/// Channels khác với Conversations: discoverable, scalable, có moderators
/// </summary>
public sealed class Channel : AggregateRoot<ChannelId>
{
    private readonly List<Guid> _members = new();
    private readonly List<Guid> _moderators = new();

    /// <summary>
    /// Tên channel (unique trong scope của workspace/organization)
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Mô tả channel (optional)
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Loại channel (Public/Private)
    /// </summary>
    public ChannelType Type { get; private set; }

    /// <summary>
    /// Owner của channel (người tạo, có quyền cao nhất)
    /// </summary>
    public Guid OwnerId { get; private set; }

    /// <summary>
    /// Thời gian tạo channel
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Channel có bị archive không
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Thời gian archive
    /// </summary>
    public DateTime? ArchivedAt { get; private set; }

    /// <summary>
    /// Danh sách member IDs
    /// </summary>
    public IReadOnlyList<Guid> Members => _members.AsReadOnly();

    /// <summary>
    /// Danh sách moderator IDs
    /// </summary>
    public IReadOnlyList<Guid> Moderators => _moderators.AsReadOnly();

    private Channel() { } // EF Core

    private Channel(
        ChannelId id,
        string name,
        string? description,
        ChannelType type,
        Guid ownerId,
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        OwnerId = ownerId;
        CreatedAt = createdAt;
        IsArchived = false;

        // Owner is automatically a member and moderator
        _members.Add(ownerId);
        _moderators.Add(ownerId);
    }

    /// <summary>
    /// Tạo channel mới
    /// </summary>
    public static Result<Channel> Create(
        string name,
        ChannelType type,
        Guid ownerId,
        string? description = null)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Channel>(new Error("Channel.InvalidName", "Channel name cannot be empty"));
        }

        if (name.Length < 3)
        {
            return Result.Failure<Channel>(new Error("Channel.NameTooShort", "Channel name must be at least 3 characters"));
        }

        if (name.Length > 100)
        {
            return Result.Failure<Channel>(new Error("Channel.NameTooLong", "Channel name cannot exceed 100 characters"));
        }

        // Validate owner
        if (ownerId == Guid.Empty)
        {
            return Result.Failure<Channel>(new Error("Channel.InvalidOwner", "Owner ID cannot be empty"));
        }

        // Validate description
        if (description != null && description.Length > 500)
        {
            return Result.Failure<Channel>(new Error("Channel.DescriptionTooLong", "Channel description cannot exceed 500 characters"));
        }

        var channelId = ChannelId.CreateUnique();
        var createdAt = DateTime.UtcNow;

        var channel = new Channel(
            channelId,
            name.Trim(),
            description?.Trim(),
            type,
            ownerId,
            createdAt);

        // Raise domain event
        channel.AddDomainEvent(new ChannelCreatedEvent(
            channelId.Value,
            name.Trim(),
            description?.Trim(),
            type,
            ownerId,
            createdAt));

        return Result.Success(channel);
    }

    /// <summary>
    /// User join channel
    /// </summary>
    public Result Join(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(new Error("Channel.InvalidUser", "User ID cannot be empty"));
        }

        // Cannot join archived channel
        if (IsArchived)
        {
            return Result.Failure(new Error("Channel.Archived", "Cannot join archived channel"));
        }

        // Already a member
        if (_members.Contains(userId))
        {
            return Result.Failure(new Error("Channel.AlreadyMember", "User is already a member"));
        }

        _members.Add(userId);

        // Raise domain event
        AddDomainEvent(new MemberJoinedChannelEvent(
            Id.Value,
            userId,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// User leave channel
    /// </summary>
    public Result Leave(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(new Error("Channel.InvalidUser", "User ID cannot be empty"));
        }

        // Owner cannot leave (must transfer ownership first)
        if (userId == OwnerId)
        {
            return Result.Failure(new Error("Channel.OwnerCannotLeave", "Owner cannot leave the channel. Transfer ownership first."));
        }

        // Not a member
        if (!_members.Contains(userId))
        {
            return Result.Failure(new Error("Channel.NotMember", "User is not a member"));
        }

        _members.Remove(userId);

        // If user was a moderator, remove from moderators too
        if (_moderators.Contains(userId))
        {
            _moderators.Remove(userId);
        }

        // Raise domain event
        AddDomainEvent(new MemberLeftChannelEvent(
            Id.Value,
            userId,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Add moderator (only owner can do this)
    /// </summary>
    public Result AddModerator(Guid userId, Guid addedBy)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(new Error("Channel.InvalidUser", "User ID cannot be empty"));
        }

        if (addedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Channel.InvalidUser", "Added by ID cannot be empty"));
        }

        // Only owner can add moderators
        if (addedBy != OwnerId)
        {
            return Result.Failure(new Error("Channel.NotOwner", "Only the owner can add moderators"));
        }

        // Cannot add moderator to archived channel
        if (IsArchived)
        {
            return Result.Failure(new Error("Channel.Archived", "Cannot add moderator to archived channel"));
        }

        // User must be a member
        if (!_members.Contains(userId))
        {
            return Result.Failure(new Error("Channel.NotMember", "User must be a member to become moderator"));
        }

        // Already a moderator
        if (_moderators.Contains(userId))
        {
            return Result.Failure(new Error("Channel.AlreadyModerator", "User is already a moderator"));
        }

        _moderators.Add(userId);

        // Raise domain event
        AddDomainEvent(new ModeratorAddedEvent(
            Id.Value,
            userId,
            addedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Remove moderator (only owner can do this)
    /// </summary>
    public Result RemoveModerator(Guid userId, Guid removedBy)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure(new Error("Channel.InvalidUser", "User ID cannot be empty"));
        }

        if (removedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Channel.InvalidUser", "Removed by ID cannot be empty"));
        }

        // Only owner can remove moderators
        if (removedBy != OwnerId)
        {
            return Result.Failure(new Error("Channel.NotOwner", "Only the owner can remove moderators"));
        }

        // Cannot remove owner as moderator
        if (userId == OwnerId)
        {
            return Result.Failure(new Error("Channel.CannotRemoveOwner", "Cannot remove owner as moderator"));
        }

        // Not a moderator
        if (!_moderators.Contains(userId))
        {
            return Result.Failure(new Error("Channel.NotModerator", "User is not a moderator"));
        }

        _moderators.Remove(userId);

        // Raise domain event
        AddDomainEvent(new ModeratorRemovedEvent(
            Id.Value,
            userId,
            removedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Update channel info (name, description) - only owner and moderators can do this
    /// </summary>
    public Result UpdateInfo(string newName, string? newDescription, Guid updatedBy)
    {
        if (updatedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Channel.InvalidUser", "Updated by ID cannot be empty"));
        }

        // Only owner and moderators can update
        if (!_moderators.Contains(updatedBy))
        {
            return Result.Failure(new Error("Channel.NotModerator", "Only owner and moderators can update channel info"));
        }

        // Cannot update archived channel
        if (IsArchived)
        {
            return Result.Failure(new Error("Channel.Archived", "Cannot update archived channel"));
        }

        // Validate new name
        if (string.IsNullOrWhiteSpace(newName))
        {
            return Result.Failure(new Error("Channel.InvalidName", "Channel name cannot be empty"));
        }

        if (newName.Length < 3)
        {
            return Result.Failure(new Error("Channel.NameTooShort", "Channel name must be at least 3 characters"));
        }

        if (newName.Length > 100)
        {
            return Result.Failure(new Error("Channel.NameTooLong", "Channel name cannot exceed 100 characters"));
        }

        // Validate new description
        if (newDescription != null && newDescription.Length > 500)
        {
            return Result.Failure(new Error("Channel.DescriptionTooLong", "Channel description cannot exceed 500 characters"));
        }

        Name = newName.Trim();
        Description = newDescription?.Trim();

        // Raise domain event
        AddDomainEvent(new ChannelUpdatedEvent(
            Id.Value,
            Name,
            Description,
            updatedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Archive channel - only owner and moderators can do this
    /// </summary>
    public Result Archive(Guid archivedBy)
    {
        if (archivedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Channel.InvalidUser", "Archived by ID cannot be empty"));
        }

        // Only owner and moderators can archive
        if (!_moderators.Contains(archivedBy))
        {
            return Result.Failure(new Error("Channel.NotModerator", "Only owner and moderators can archive channel"));
        }

        if (IsArchived)
        {
            return Result.Failure(new Error("Channel.AlreadyArchived", "Channel is already archived"));
        }

        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new ChannelArchivedEvent(
            Id.Value,
            archivedBy,
            ArchivedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Check if user is a member
    /// </summary>
    public bool IsMember(Guid userId) => _members.Contains(userId);

    /// <summary>
    /// Check if user is a moderator
    /// </summary>
    public bool IsModerator(Guid userId) => _moderators.Contains(userId);

    /// <summary>
    /// Check if user is the owner
    /// </summary>
    public bool IsOwner(Guid userId) => userId == OwnerId;
}
