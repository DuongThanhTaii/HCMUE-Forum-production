using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Conversations.Events;

namespace UniHub.Chat.Domain.Tests.Conversations;

public class ConversationTests
{
    private readonly Guid _user1 = Guid.NewGuid();
    private readonly Guid _user2 = Guid.NewGuid();
    private readonly Guid _user3 = Guid.NewGuid();

    #region CreateDirect Tests

    [Fact]
    public void CreateDirect_WithValidData_ShouldSucceed()
    {
        // Act
        var result = Conversation.CreateDirect(_user1, _user2, _user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var conversation = result.Value;
        conversation.Type.Should().Be(ConversationType.Direct);
        conversation.Participants.Should().HaveCount(2);
        conversation.Participants.Should().Contain(_user1);
        conversation.Participants.Should().Contain(_user2);
        conversation.Title.Should().BeNull();
        conversation.CreatedBy.Should().Be(_user1);
        conversation.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void CreateDirect_WithUser2AsCreator_ShouldSucceed()
    {
        // Act
        var result = Conversation.CreateDirect(_user1, _user2, _user2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedBy.Should().Be(_user2);
    }

    [Fact]
    public void CreateDirect_ShouldRaiseConversationCreatedEvent()
    {
        // Act
        var result = Conversation.CreateDirect(_user1, _user2, _user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var events = result.Value.DomainEvents;
        events.Should().ContainSingle();
        
        var @event = events.First() as ConversationCreatedEvent;
        @event.Should().NotBeNull();
        @event!.Type.Should().Be(ConversationType.Direct);
        @event.ParticipantIds.Should().HaveCount(2);
        @event.ParticipantIds.Should().Contain(_user1);
        @event.ParticipantIds.Should().Contain(_user2);
        @event.Title.Should().BeNull();
        @event.CreatedBy.Should().Be(_user1);
    }

    [Fact]
    public void CreateDirect_WithEmptyUser1_ShouldFail()
    {
        // Act
        var result = Conversation.CreateDirect(Guid.Empty, _user2, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidUser");
    }

    [Fact]
    public void CreateDirect_WithEmptyUser2_ShouldFail()
    {
        // Act
        var result = Conversation.CreateDirect(_user1, Guid.Empty, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidUser");
    }

    [Fact]
    public void CreateDirect_WithEmptyCreator_ShouldFail()
    {
        // Act
        var result = Conversation.CreateDirect(_user1, _user2, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidCreator");
    }

    [Fact]
    public void CreateDirect_WithSameUser_ShouldFail()
    {
        // Act
        var result = Conversation.CreateDirect(_user1, _user1, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.SameUsers");
    }

    [Fact]
    public void CreateDirect_WithCreatorNotParticipant_ShouldFail()
    {
        // Act
        var result = Conversation.CreateDirect(_user1, _user2, _user3);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.CreatorNotParticipant");
    }

    #endregion

    #region CreateGroup Tests

    [Fact]
    public void CreateGroup_WithValidData_ShouldSucceed()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };

        // Act
        var result = Conversation.CreateGroup("Study Group", participants, _user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var conversation = result.Value;
        conversation.Type.Should().Be(ConversationType.Group);
        conversation.Title.Should().Be("Study Group");
        conversation.Participants.Should().HaveCount(3);
        conversation.Participants.Should().Contain(_user1);
        conversation.Participants.Should().Contain(_user2);
        conversation.Participants.Should().Contain(_user3);
        conversation.CreatedBy.Should().Be(_user1);
        conversation.IsArchived.Should().BeFalse();
    }

    [Fact]
    public void CreateGroup_ShouldRaiseConversationCreatedEvent()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };

        // Act
        var result = Conversation.CreateGroup("Project Team", participants, _user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var events = result.Value.DomainEvents;
        events.Should().ContainSingle();
        
        var @event = events.First() as ConversationCreatedEvent;
        @event.Should().NotBeNull();
        @event!.Type.Should().Be(ConversationType.Group);
        @event.Title.Should().Be("Project Team");
        @event.ParticipantIds.Should().HaveCount(3);
        @event.CreatedBy.Should().Be(_user1);
    }

    [Fact]
    public void CreateGroup_WithNullTitle_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };

        // Act
        var result = Conversation.CreateGroup(null!, participants, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.MissingTitle");
    }

    [Fact]
    public void CreateGroup_WithEmptyTitle_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };

        // Act
        var result = Conversation.CreateGroup("   ", participants, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.MissingTitle");
    }

    [Fact]
    public void CreateGroup_WithTitleTooShort_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };

        // Act
        var result = Conversation.CreateGroup("AB", participants, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.TitleTooShort");
    }

    [Fact]
    public void CreateGroup_WithTitleTooLong_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };
        var longTitle = new string('A', 101);

        // Act
        var result = Conversation.CreateGroup(longTitle, participants, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.TitleTooLong");
    }

    [Fact]
    public void CreateGroup_WithTitleHavingWhitespace_ShouldTrimTitle()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };

        // Act
        var result = Conversation.CreateGroup("  Study Group  ", participants, _user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Study Group");
    }

    [Fact]
    public void CreateGroup_WithEmptyCreator_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };

        // Act
        var result = Conversation.CreateGroup("Group", participants, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidCreator");
    }

    [Fact]
    public void CreateGroup_WithOneParticipant_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1 };

        // Act
        var result = Conversation.CreateGroup("Group", participants, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InsufficientParticipants");
    }

    [Fact]
    public void CreateGroup_WithEmptyParticipantList_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid>();

        // Act
        var result = Conversation.CreateGroup("Group", participants, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InsufficientParticipants");
    }

    [Fact]
    public void CreateGroup_WithNullParticipantList_ShouldFail()
    {
        // Act
        var result = Conversation.CreateGroup("Group", null!, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InsufficientParticipants");
    }

    [Fact]
    public void CreateGroup_WithEmptyParticipantId_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, Guid.Empty };

        // Act
        var result = Conversation.CreateGroup("Group", participants, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidParticipant");
    }

    [Fact]
    public void CreateGroup_WithDuplicateParticipants_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user1 };

        // Act
        var result = Conversation.CreateGroup("Group", participants, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.DuplicateParticipants");
    }

    [Fact]
    public void CreateGroup_WithCreatorNotInParticipants_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };

        // Act
        var result = Conversation.CreateGroup("Group", participants, _user3);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.CreatorNotParticipant");
    }

    #endregion

    #region AddParticipant Tests

    [Fact]
    public void AddParticipant_ToGroupConversation_ShouldSucceed()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;
        conversation.ClearDomainEvents(); // Clear creation event

        var newParticipant = _user3;

        // Act
        var result = conversation.AddParticipant(newParticipant, _user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        conversation.Participants.Should().HaveCount(3);
        conversation.Participants.Should().Contain(newParticipant);
        
        var events = conversation.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as ParticipantAddedEvent;
        @event.Should().NotBeNull();
        @event!.ParticipantId.Should().Be(newParticipant);
        @event.AddedBy.Should().Be(_user1);
    }

    [Fact]
    public void AddParticipant_ToDirectConversation_ShouldFail()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;

        // Act
        var result = conversation.AddParticipant(_user3, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotGroupChat");
    }

    [Fact]
    public void AddParticipant_WithEmptyParticipantId_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;

        // Act
        var result = conversation.AddParticipant(Guid.Empty, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidParticipant");
    }

    [Fact]
    public void AddParticipant_WithEmptyAddedBy_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;

        // Act
        var result = conversation.AddParticipant(_user3, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidUser");
    }

    [Fact]
    public void AddParticipant_ByNonParticipant_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;

        // Act
        var result = conversation.AddParticipant(Guid.NewGuid(), _user3);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotParticipant");
    }

    [Fact]
    public void AddParticipant_WhenAlreadyParticipant_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;

        // Act
        var result = conversation.AddParticipant(_user2, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.AlreadyParticipant");
    }

    [Fact]
    public void AddParticipant_ToArchivedConversation_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;
        conversation.Archive(_user1);

        // Act
        var result = conversation.AddParticipant(_user3, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.Archived");
    }

    #endregion

    #region RemoveParticipant Tests

    [Fact]
    public void RemoveParticipant_FromGroupConversation_ShouldSucceed()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;
        conversation.ClearDomainEvents();

        // Act
        var result = conversation.RemoveParticipant(_user3, _user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        conversation.Participants.Should().HaveCount(2);
        conversation.Participants.Should().NotContain(_user3);
        
        var events = conversation.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as ParticipantRemovedEvent;
        @event.Should().NotBeNull();
        @event!.ParticipantId.Should().Be(_user3);
        @event.RemovedBy.Should().Be(_user1);
    }

    [Fact]
    public void RemoveParticipant_SelfRemove_ShouldSucceed()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;

        // Act
        var result = conversation.RemoveParticipant(_user3, _user3); // User removes themselves

        // Assert
        result.IsSuccess.Should().BeTrue();
        conversation.Participants.Should().NotContain(_user3);
    }

    [Fact]
    public void RemoveParticipant_FromDirectConversation_ShouldFail()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;

        // Act
        var result = conversation.RemoveParticipant(_user2, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotGroupChat");
    }

    [Fact]
    public void RemoveParticipant_WithEmptyParticipantId_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;

        // Act
        var result = conversation.RemoveParticipant(Guid.Empty, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidParticipant");
    }

    [Fact]
    public void RemoveParticipant_WithEmptyRemovedBy_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;

        // Act
        var result = conversation.RemoveParticipant(_user3, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidUser");
    }

    [Fact]
    public void RemoveParticipant_ByNonParticipant_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;
        var outsider = Guid.NewGuid();

        // Act
        var result = conversation.RemoveParticipant(_user3, outsider);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotParticipant");
    }

    [Fact]
    public void RemoveParticipant_WhenNotParticipant_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;
        var outsider = Guid.NewGuid();

        // Act
        var result = conversation.RemoveParticipant(outsider, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotAParticipant");
    }

    [Fact]
    public void RemoveParticipant_WhenOnlyTwoParticipants_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;

        // Act
        var result = conversation.RemoveParticipant(_user2, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.MinimumParticipants");
    }

    [Fact]
    public void RemoveParticipant_FromArchivedConversation_ShouldFail()
    {
        // Arrange
        var participants = new List<Guid> { _user1, _user2, _user3 };
        var conversation = Conversation.CreateGroup("Group", participants, _user1).Value;
        conversation.Archive(_user1);

        // Act
        var result = conversation.RemoveParticipant(_user3, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.Archived");
    }

    #endregion

    #region Archive Tests

    [Fact]
    public void Archive_ByParticipant_ShouldSucceed()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;
        conversation.ClearDomainEvents();

        // Act
        var result = conversation.Archive(_user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        conversation.IsArchived.Should().BeTrue();
        
        var events = conversation.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as ConversationArchivedEvent;
        @event.Should().NotBeNull();
        @event!.ArchivedBy.Should().Be(_user1);
    }

    [Fact]
    public void Archive_WithEmptyArchivedBy_ShouldFail()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;

        // Act
        var result = conversation.Archive(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidUser");
    }

    [Fact]
    public void Archive_ByNonParticipant_ShouldFail()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;

        // Act
        var result = conversation.Archive(_user3);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotParticipant");
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldFail()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;
        conversation.Archive(_user1);

        // Act
        var result = conversation.Archive(_user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.AlreadyArchived");
    }

    #endregion

    #region Unarchive Tests

    [Fact]
    public void Unarchive_ArchivedConversation_ShouldSucceed()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;
        conversation.Archive(_user1);
        conversation.ClearDomainEvents();

        // Act
        var result = conversation.Unarchive(_user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        conversation.IsArchived.Should().BeFalse();
        
        var events = conversation.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as ConversationUnarchivedEvent;
        @event.Should().NotBeNull();
        @event!.UnarchivedBy.Should().Be(_user1);
    }

    [Fact]
    public void Unarchive_WithEmptyUnarchivedBy_ShouldFail()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;
        conversation.Archive(_user1);

        // Act
        var result = conversation.Unarchive(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.InvalidUser");
    }

    [Fact]
    public void Unarchive_ByNonParticipant_ShouldFail()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;
        conversation.Archive(_user1);

        // Act
        var result = conversation.Unarchive(_user3);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotParticipant");
    }

    [Fact]
    public void Unarchive_WhenNotArchived_ShouldFail()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;

        // Act
        var result = conversation.Unarchive(_user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Conversation.NotArchived");
    }

    #endregion

    #region UpdateLastMessageTime Tests

    [Fact]
    public void UpdateLastMessageTime_ShouldUpdateProperty()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;
        var messageTime = DateTime.UtcNow;

        // Act
        conversation.UpdateLastMessageTime(messageTime);

        // Assert
        conversation.LastMessageAt.Should().Be(messageTime);
    }

    [Fact]
    public void UpdateLastMessageTime_MultipleTimes_ShouldUpdateToLatest()
    {
        // Arrange
        var conversation = Conversation.CreateDirect(_user1, _user2, _user1).Value;
        var time1 = DateTime.UtcNow.AddMinutes(-5);
        var time2 = DateTime.UtcNow;

        // Act
        conversation.UpdateLastMessageTime(time1);
        conversation.UpdateLastMessageTime(time2);

        // Assert
        conversation.LastMessageAt.Should().Be(time2);
    }

    #endregion
}
