using UniHub.Chat.Domain.Channels;
using UniHub.Chat.Domain.Channels.Events;

namespace UniHub.Chat.Domain.Tests.Channels;

public class ChannelTests
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _user1 = Guid.NewGuid();
    private readonly Guid _user2 = Guid.NewGuid();
    private readonly Guid _user3 = Guid.NewGuid();

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var result = Channel.Create("General", ChannelType.Public, _ownerId, "Main discussion channel");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var channel = result.Value;
        channel.Name.Should().Be("General");
        channel.Description.Should().Be("Main discussion channel");
        channel.Type.Should().Be(ChannelType.Public);
        channel.OwnerId.Should().Be(_ownerId);
        channel.IsArchived.Should().BeFalse();
        channel.Members.Should().ContainSingle().Which.Should().Be(_ownerId); // Owner is automatically a member
        channel.Moderators.Should().ContainSingle().Which.Should().Be(_ownerId); // Owner is automatically a moderator
    }

    [Fact]
    public void Create_WithoutDescription_ShouldSucceed()
    {
        // Act
        var result = Channel.Create("Random", ChannelType.Public, _ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldTrimNameAndDescription()
    {
        // Act
        var result = Channel.Create("  General  ", ChannelType.Private, _ownerId, "  Description  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("General");
        result.Value.Description.Should().Be("Description");
    }

    [Fact]
    public void Create_ShouldRaiseChannelCreatedEvent()
    {
        // Act
        var result = Channel.Create("General", ChannelType.Public, _ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var events = result.Value.DomainEvents;
        events.Should().ContainSingle();
        
        var @event = events.First() as ChannelCreatedEvent;
        @event.Should().NotBeNull();
        @event!.Name.Should().Be("General");
        @event.Type.Should().Be(ChannelType.Public);
        @event.CreatedBy.Should().Be(_ownerId);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        // Act
        var result = Channel.Create("   ", ChannelType.Public, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.InvalidName");
    }

    [Fact]
    public void Create_WithNameTooShort_ShouldFail()
    {
        // Act
        var result = Channel.Create("AB", ChannelType.Public, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NameTooShort");
    }

    [Fact]
    public void Create_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var result = Channel.Create(longName, ChannelType.Public, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NameTooLong");
    }

    [Fact]
    public void Create_WithEmptyOwnerId_ShouldFail()
    {
        // Act
        var result = Channel.Create("General", ChannelType.Public, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.InvalidOwner");
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var longDescription = new string('A', 501);

        // Act
        var result = Channel.Create("General", ChannelType.Public, _ownerId, longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.DescriptionTooLong");
    }

    #endregion

    #region Join Tests

    [Fact]
    public void Join_WithValidUser_ShouldSucceed()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.ClearDomainEvents();

        // Act
        var result = channel.Join(_user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.Members.Should().HaveCount(2);
        channel.Members.Should().Contain(_user1);
        
        var events = channel.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as MemberJoinedChannelEvent;
        @event.Should().NotBeNull();
        @event!.MemberId.Should().Be(_user1);
    }

    [Fact]
    public void Join_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.Join(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.InvalidUser");
    }

    [Fact]
    public void Join_WhenAlreadyMember_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);

        // Act
        var result = channel.Join(_user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.AlreadyMember");
    }

    [Fact]
    public void Join_ArchivedChannel_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Archive(_ownerId);

        // Act
        var result = channel.Join(_user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.Archived");
    }

    #endregion

    #region Leave Tests

    [Fact]
    public void Leave_WithValidUser_ShouldSucceed()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.ClearDomainEvents();

        // Act
        var result = channel.Leave(_user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.Members.Should().ContainSingle().Which.Should().Be(_ownerId);
        channel.Members.Should().NotContain(_user1);
        
        var events = channel.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as MemberLeftChannelEvent;
        @event.Should().NotBeNull();
        @event!.MemberId.Should().Be(_user1);
    }

    [Fact]
    public void Leave_WhenUserIsAlsoModerator_ShouldRemoveFromBoth()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.AddModerator(_user1, _ownerId);

        // Act
        var result = channel.Leave(_user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.Members.Should().NotContain(_user1);
        channel.Moderators.Should().NotContain(_user1);
    }

    [Fact]
    public void Leave_ByOwner_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.Leave(_ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.OwnerCannotLeave");
    }

    [Fact]
    public void Leave_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.Leave(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.InvalidUser");
    }

    [Fact]
    public void Leave_WhenNotMember_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.Leave(_user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NotMember");
    }

    #endregion

    #region AddModerator Tests

    [Fact]
    public void AddModerator_ByOwner_ShouldSucceed()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.ClearDomainEvents();

        // Act
        var result = channel.AddModerator(_user1, _ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.Moderators.Should().HaveCount(2);
        channel.Moderators.Should().Contain(_user1);
        
        var events = channel.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as ModeratorAddedEvent;
        @event.Should().NotBeNull();
        @event!.ModeratorId.Should().Be(_user1);
        @event.AddedBy.Should().Be(_ownerId);
    }

    [Fact]
    public void AddModerator_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.AddModerator(Guid.Empty, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.InvalidUser");
    }

    [Fact]
    public void AddModerator_ByNonOwner_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.Join(_user2);

        // Act
        var result = channel.AddModerator(_user2, _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NotOwner");
    }

    [Fact]
    public void AddModerator_WhenNotMember_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.AddModerator(_user1, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NotMember");
    }

    [Fact]
    public void AddModerator_WhenAlreadyModerator_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.AddModerator(_user1, _ownerId);

        // Act
        var result = channel.AddModerator(_user1, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.AlreadyModerator");
    }

    [Fact]
    public void AddModerator_ToArchivedChannel_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.Archive(_ownerId);

        // Act
        var result = channel.AddModerator(_user1, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.Archived");
    }

    #endregion

    #region RemoveModerator Tests

    [Fact]
    public void RemoveModerator_ByOwner_ShouldSucceed()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.AddModerator(_user1, _ownerId);
        channel.ClearDomainEvents();

        // Act
        var result = channel.RemoveModerator(_user1, _ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.Moderators.Should().ContainSingle().Which.Should().Be(_ownerId);
        channel.Moderators.Should().NotContain(_user1);
        
        var events = channel.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as ModeratorRemovedEvent;
        @event.Should().NotBeNull();
        @event!.ModeratorId.Should().Be(_user1);
        @event.RemovedBy.Should().Be(_ownerId);
    }

    [Fact]
    public void RemoveModerator_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.RemoveModerator(Guid.Empty, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.InvalidUser");
    }

    [Fact]
    public void RemoveModerator_ByNonOwner_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.Join(_user2);
        channel.AddModerator(_user1, _ownerId);

        // Act
        var result = channel.RemoveModerator(_user1, _user2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NotOwner");
    }

    [Fact]
    public void RemoveModerator_Owner_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.RemoveModerator(_ownerId, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.CannotRemoveOwner");
    }

    [Fact]
    public void RemoveModerator_WhenNotModerator_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);

        // Act
        var result = channel.RemoveModerator(_user1, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NotModerator");
    }

    #endregion

    #region UpdateInfo Tests

    [Fact]
    public void UpdateInfo_ByOwner_ShouldSucceed()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId, "Old description").Value;
        channel.ClearDomainEvents();

        // Act
        var result = channel.UpdateInfo("NewGeneral", "New description", _ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.Name.Should().Be("NewGeneral");
        channel.Description.Should().Be("New description");
        
        var events = channel.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as ChannelUpdatedEvent;
        @event.Should().NotBeNull();
        @event!.NewName.Should().Be("NewGeneral");
        @event.NewDescription.Should().Be("New description");
        @event.UpdatedBy.Should().Be(_ownerId);
    }

    [Fact]
    public void UpdateInfo_ByModerator_ShouldSucceed()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.AddModerator(_user1, _ownerId);

        // Act
        var result = channel.UpdateInfo("Updated", "Updated description", _user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.Name.Should().Be("Updated");
    }

    [Fact]
    public void UpdateInfo_ShouldTrimValues()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.UpdateInfo("  NewName  ", "  NewDesc  ", _ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.Name.Should().Be("NewName");
        channel.Description.Should().Be("NewDesc");
    }

    [Fact]
    public void UpdateInfo_ByNonModerator_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);

        // Act
        var result = channel.UpdateInfo("NewName", "NewDesc", _user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NotModerator");
    }

    [Fact]
    public void UpdateInfo_ArchivedChannel_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Archive(_ownerId);

        // Act
        var result = channel.UpdateInfo("NewName", "NewDesc", _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.Archived");
    }

    [Fact]
    public void UpdateInfo_WithEmptyName_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.UpdateInfo("   ", "Description", _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.InvalidName");
    }

    [Fact]
    public void UpdateInfo_WithNameTooShort_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.UpdateInfo("AB", "Description", _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NameTooShort");
    }

    [Fact]
    public void UpdateInfo_WithNameTooLong_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        var longName = new string('A', 101);

        // Act
        var result = channel.UpdateInfo(longName, "Description", _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NameTooLong");
    }

    [Fact]
    public void UpdateInfo_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        var longDescription = new string('A', 501);

        // Act
        var result = channel.UpdateInfo("NewName", longDescription, _ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.DescriptionTooLong");
    }

    #endregion

    #region Archive Tests

    [Fact]
    public void Archive_ByOwner_ShouldSucceed()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.ClearDomainEvents();

        // Act
        var result = channel.Archive(_ownerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.IsArchived.Should().BeTrue();
        channel.ArchivedAt.Should().NotBeNull();
        channel.ArchivedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        
        var events = channel.DomainEvents;
        events.Should().ContainSingle();
        var @event = events.First() as ChannelArchivedEvent;
        @event.Should().NotBeNull();
        @event!.ArchivedBy.Should().Be(_ownerId);
    }

    [Fact]
    public void Archive_ByModerator_ShouldSucceed()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.AddModerator(_user1, _ownerId);

        // Act
        var result = channel.Archive(_user1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        channel.IsArchived.Should().BeTrue();
    }

    [Fact]
    public void Archive_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act
        var result = channel.Archive(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.InvalidUser");
    }

    [Fact]
    public void Archive_ByNonModerator_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);

        // Act
        var result = channel.Archive(_user1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.NotModerator");
    }

    [Fact]
    public void Archive_AlreadyArchived_ShouldFail()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Archive(_ownerId);

        // Act
        var result = channel.Archive(_ownerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Channel.AlreadyArchived");
    }

    #endregion

    #region Helper Methods Tests

    [Fact]
    public void IsMember_WithMember_ShouldReturnTrue()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);

        // Act & Assert
        channel.IsMember(_user1).Should().BeTrue();
        channel.IsMember(_ownerId).Should().BeTrue();
    }

    [Fact]
    public void IsMember_WithNonMember_ShouldReturnFalse()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act & Assert
        channel.IsMember(_user1).Should().BeFalse();
    }

    [Fact]
    public void IsModerator_WithModerator_ShouldReturnTrue()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);
        channel.AddModerator(_user1, _ownerId);

        // Act & Assert
        channel.IsModerator(_user1).Should().BeTrue();
        channel.IsModerator(_ownerId).Should().BeTrue();
    }

    [Fact]
    public void IsModerator_WithNonModerator_ShouldReturnFalse()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;
        channel.Join(_user1);

        // Act & Assert
        channel.IsModerator(_user1).Should().BeFalse();
    }

    [Fact]
    public void IsOwner_WithOwner_ShouldReturnTrue()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act & Assert
        channel.IsOwner(_ownerId).Should().BeTrue();
    }

    [Fact]
    public void IsOwner_WithNonOwner_ShouldReturnFalse()
    {
        // Arrange
        var channel = Channel.Create("General", ChannelType.Public, _ownerId).Value;

        // Act & Assert
        channel.IsOwner(_user1).Should().BeFalse();
    }

    #endregion
}
