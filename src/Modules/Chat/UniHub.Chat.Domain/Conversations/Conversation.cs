using UniHub.Chat.Domain.Conversations.Events;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Domain.Conversations;

/// <summary>
/// Conversation aggregate - đại diện cho một cuộc hội thoại (1:1 hoặc group chat)
/// </summary>
public sealed class Conversation : AggregateRoot<ConversationId>
{
    private readonly List<Guid> _participants = new();

    /// <summary>
    /// Loại conversation (Direct hoặc Group)
    /// </summary>
    public ConversationType Type { get; private set; }

    /// <summary>
    /// Tiêu đề conversation (bắt buộc cho Group, optional cho Direct)
    /// </summary>
    public string? Title { get; private set; }

    /// <summary>
    /// Danh sách user IDs tham gia conversation (read-only)
    /// </summary>
    public IReadOnlyList<Guid> Participants => _participants.AsReadOnly();

    /// <summary>
    /// Người tạo conversation
    /// </summary>
    public Guid CreatedBy { get; private set; }

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Thời gian message cuối cùng (null nếu chưa có message)
    /// </summary>
    public DateTime? LastMessageAt { get; private set; }

    /// <summary>
    /// Conversation có bị archive không
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Private constructor for EF Core
    /// </summary>
    private Conversation() { }

    /// <summary>
    /// Private constructor for factory method
    /// </summary>
    private Conversation(
        ConversationId id,
        ConversationType type,
        List<Guid> participants,
        string? title,
        Guid createdBy,
        DateTime createdAt)
    {
        Id = id;
        Type = type;
        _participants = participants;
        Title = title;
        CreatedBy = createdBy;
        CreatedAt = createdAt;
        IsArchived = false;
    }

    /// <summary>
    /// Tạo Direct conversation (1:1 chat)
    /// </summary>
    public static Result<Conversation> CreateDirect(
        Guid user1Id,
        Guid user2Id,
        Guid createdBy)
    {
        // Validate IDs
        if (user1Id == Guid.Empty)
        {
            return Result.Failure<Conversation>(new Error("Conversation.InvalidUser", "User 1 ID cannot be empty"));
        }

        if (user2Id == Guid.Empty)
        {
            return Result.Failure<Conversation>(new Error("Conversation.InvalidUser", "User 2 ID cannot be empty"));
        }

        if (createdBy == Guid.Empty)
        {
            return Result.Failure<Conversation>(new Error("Conversation.InvalidCreator", "Creator ID cannot be empty"));
        }

        // Direct chat không thể có 2 user giống nhau
        if (user1Id == user2Id)
        {
            return Result.Failure<Conversation>(new Error("Conversation.SameUsers", "Cannot create direct conversation with the same user"));
        }

        // Creator phải là một trong 2 participants
        if (createdBy != user1Id && createdBy != user2Id)
        {
            return Result.Failure<Conversation>(new Error("Conversation.CreatorNotParticipant", "Creator must be one of the participants"));
        }

        var conversationId = ConversationId.CreateUnique();
        var participants = new List<Guid> { user1Id, user2Id };
        var createdAt = DateTime.UtcNow;

        var conversation = new Conversation(
            conversationId,
            ConversationType.Direct,
            participants,
            null, // Direct chat không có title
            createdBy,
            createdAt);

        // Raise domain event
        conversation.AddDomainEvent(new ConversationCreatedEvent(
            conversationId.Value,
            ConversationType.Direct,
            participants,
            null,
            createdBy,
            createdAt));

        return Result.Success(conversation);
    }

    /// <summary>
    /// Tạo Group conversation
    /// </summary>
    public static Result<Conversation> CreateGroup(
        string title,
        List<Guid> participantIds,
        Guid createdBy)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure<Conversation>(new Error("Conversation.MissingTitle", "Group conversation must have a title"));
        }

        if (title.Length < 3)
        {
            return Result.Failure<Conversation>(new Error("Conversation.TitleTooShort", "Title must be at least 3 characters"));
        }

        if (title.Length > 100)
        {
            return Result.Failure<Conversation>(new Error("Conversation.TitleTooLong", "Title cannot exceed 100 characters"));
        }

        // Validate creator
        if (createdBy == Guid.Empty)
        {
            return Result.Failure<Conversation>(new Error("Conversation.InvalidCreator", "Creator ID cannot be empty"));
        }

        // Validate participants
        if (participantIds == null || participantIds.Count < 2)
        {
            return Result.Failure<Conversation>(new Error("Conversation.InsufficientParticipants", "Group conversation must have at least 2 participants"));
        }

        // Check for empty IDs
        if (participantIds.Any(id => id == Guid.Empty))
        {
            return Result.Failure<Conversation>(new Error("Conversation.InvalidParticipant", "Participant IDs cannot be empty"));
        }

        // Check for duplicates
        if (participantIds.Distinct().Count() != participantIds.Count)
        {
            return Result.Failure<Conversation>(new Error("Conversation.DuplicateParticipants", "Cannot add duplicate participants"));
        }

        // Creator phải là một trong các participants
        if (!participantIds.Contains(createdBy))
        {
            return Result.Failure<Conversation>(new Error("Conversation.CreatorNotParticipant", "Creator must be one of the participants"));
        }

        var conversationId = ConversationId.CreateUnique();
        var createdAt = DateTime.UtcNow;

        var conversation = new Conversation(
            conversationId,
            ConversationType.Group,
            new List<Guid>(participantIds),
            title.Trim(),
            createdBy,
            createdAt);

        // Raise domain event
        conversation.AddDomainEvent(new ConversationCreatedEvent(
            conversationId.Value,
            ConversationType.Group,
            participantIds,
            title.Trim(),
            createdBy,
            createdAt));

        return Result.Success(conversation);
    }

    /// <summary>
    /// Thêm participant vào group conversation
    /// </summary>
    public Result AddParticipant(Guid participantId, Guid addedBy)
    {
        // Validate
        if (participantId == Guid.Empty)
        {
            return Result.Failure(new Error("Conversation.InvalidParticipant", "Participant ID cannot be empty"));
        }

        if (addedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Conversation.InvalidUser", "Added by ID cannot be empty"));
        }

        // Chỉ group chat mới có thể thêm participant
        if (Type != ConversationType.Group)
        {
            return Result.Failure(new Error("Conversation.NotGroupChat", "Can only add participants to group conversations"));
        }

        // Không thể thêm vào conversation đã archive
        if (IsArchived)
        {
            return Result.Failure(new Error("Conversation.Archived", "Cannot add participants to archived conversation"));
        }

        // Người thêm phải là participant
        if (!_participants.Contains(addedBy))
        {
            return Result.Failure(new Error("Conversation.NotParticipant", "Only participants can add new members"));
        }

        // Không thể thêm participant đã tồn tại
        if (_participants.Contains(participantId))
        {
            return Result.Failure(new Error("Conversation.AlreadyParticipant", "User is already a participant"));
        }

        _participants.Add(participantId);

        // Raise domain event
        AddDomainEvent(new ParticipantAddedEvent(
            Id.Value,
            participantId,
            addedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Remove participant khỏi group conversation
    /// </summary>
    public Result RemoveParticipant(Guid participantId, Guid removedBy)
    {
        // Validate
        if (participantId == Guid.Empty)
        {
            return Result.Failure(new Error("Conversation.InvalidParticipant", "Participant ID cannot be empty"));
        }

        if (removedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Conversation.InvalidUser", "Removed by ID cannot be empty"));
        }

        // Chỉ group chat mới có thể remove participant
        if (Type != ConversationType.Group)
        {
            return Result.Failure(new Error("Conversation.NotGroupChat", "Can only remove participants from group conversations"));
        }

        // Không thể remove khỏi conversation đã archive
        if (IsArchived)
        {
            return Result.Failure(new Error("Conversation.Archived", "Cannot remove participants from archived conversation"));
        }

        // Người remove phải là participant (có thể tự remove mình - leave conversation)
        if (!_participants.Contains(removedBy))
        {
            return Result.Failure(new Error("Conversation.NotParticipant", "Only participants can remove members"));
        }

        // Participant phải tồn tại
        if (!_participants.Contains(participantId))
        {
            return Result.Failure(new Error("Conversation.NotAParticipant", "User is not a participant"));
        }

        // Không thể remove nếu chỉ còn 2 người (minimum for group)
        if (_participants.Count <= 2)
        {
            return Result.Failure(new Error("Conversation.MinimumParticipants", "Group conversation must have at least 2 participants"));
        }

        _participants.Remove(participantId);

        // Raise domain event
        AddDomainEvent(new ParticipantRemovedEvent(
            Id.Value,
            participantId,
            removedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Archive conversation
    /// </summary>
    public Result Archive(Guid archivedBy)
    {
        if (archivedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Conversation.InvalidUser", "Archived by ID cannot be empty"));
        }

        // Người archive phải là participant
        if (!_participants.Contains(archivedBy))
        {
            return Result.Failure(new Error("Conversation.NotParticipant", "Only participants can archive conversation"));
        }

        if (IsArchived)
        {
            return Result.Failure(new Error("Conversation.AlreadyArchived", "Conversation is already archived"));
        }

        IsArchived = true;

        // Raise domain event
        AddDomainEvent(new ConversationArchivedEvent(
            Id.Value,
            archivedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Unarchive conversation
    /// </summary>
    public Result Unarchive(Guid unarchivedBy)
    {
        if (unarchivedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Conversation.InvalidUser", "Unarchived by ID cannot be empty"));
        }

        // Người unarchive phải là participant
        if (!_participants.Contains(unarchivedBy))
        {
            return Result.Failure(new Error("Conversation.NotParticipant", "Only participants can unarchive conversation"));
        }

        if (!IsArchived)
        {
            return Result.Failure(new Error("Conversation.NotArchived", "Conversation is not archived"));
        }

        IsArchived = false;

        // Raise domain event
        AddDomainEvent(new ConversationUnarchivedEvent(
            Id.Value,
            unarchivedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Update last message time (gọi khi có message mới)
    /// </summary>
    public void UpdateLastMessageTime(DateTime messageTime)
    {
        LastMessageAt = messageTime;
    }
}
