using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;
using UniHub.Chat.Domain.Messages.Events;

namespace UniHub.Chat.Domain.Tests.Messages;

public class MessageTests
{
    private readonly ConversationId _conversationId = ConversationId.CreateUnique();
    private readonly Guid _senderId = Guid.NewGuid();
    private readonly Guid _userId1 = Guid.NewGuid();
    private readonly Guid _userId2 = Guid.NewGuid();

    #region CreateText Tests

    [Fact]
    public void CreateText_WithValidData_ShouldSucceed()
    {
        // Act
        var result = Message.CreateText(_conversationId, _senderId, "Hello World");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var message = result.Value;
        message.ConversationId.Should().Be(_conversationId);
        message.SenderId.Should().Be(_senderId);
        message.Content.Should().Be("Hello World");
        message.Type.Should().Be(MessageType.Text);
        message.IsDeleted.Should().BeFalse();
        message.EditedAt.Should().BeNull();
        message.ReplyToMessageId.Should().BeNull();
        message.Attachments.Should().BeEmpty();
        message.Reactions.Should().BeEmpty();
    }

    [Fact]
    public void CreateText_WithReplyTo_ShouldSucceed()
    {
        // Arrange
        var replyToId = MessageId.CreateUnique();

        // Act
        var result = Message.CreateText(_conversationId, _senderId, "Reply message", replyToId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReplyToMessageId.Should().Be(replyToId);
    }

    [Fact]
    public void CreateText_ShouldTrimContent()
    {
        // Act
        var result = Message.CreateText(_conversationId, _senderId, "  Hello World  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be("Hello World");
    }

    [Fact]
    public void CreateText_WithNullConversationId_ShouldFail()
    {
        // Act
        var result = Message.CreateText(null!, _senderId, "Hello");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidConversation");
    }

    [Fact]
    public void CreateText_WithEmptySenderId_ShouldFail()
    {
        // Act
        var result = Message.CreateText(_conversationId, Guid.Empty, "Hello");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidSender");
    }

    [Fact]
    public void CreateText_WithEmptyContent_ShouldFail()
    {
        // Act
        var result = Message.CreateText(_conversationId, _senderId, "   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.EmptyContent");
    }

    [Fact]
    public void CreateText_WithContentTooLong_ShouldFail()
    {
        // Arrange
        var longContent = new string('A', 5001);

        // Act
        var result = Message.CreateText(_conversationId, _senderId, longContent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.ContentTooLong");
    }

    #endregion

    #region CreateWithAttachments Tests

    [Fact]
    public void CreateWithAttachments_WithValidData_ShouldSucceed()
    {
        // Arrange
        var attachment = Attachment.Create("test.pdf", "https://storage/test.pdf", 1024, "application/pdf").Value;
        var attachments = new List<Attachment> { attachment };

        // Act
        var result = Message.CreateWithAttachments(
            _conversationId,
            _senderId,
            "Check this file",
            MessageType.File,
            attachments);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var message = result.Value;
        message.Type.Should().Be(MessageType.File);
        message.Content.Should().Be("Check this file");
        message.Attachments.Should().HaveCount(1);
        message.Attachments.First().Should().Be(attachment);
    }

    [Fact]
    public void CreateWithAttachments_WithEmptyContent_ShouldSucceed()
    {
        // Arrange
        var attachment = Attachment.Create("photo.jpg", "https://storage/photo.jpg", 2048, "image/jpeg").Value;
        var attachments = new List<Attachment> { attachment };

        // Act
        var result = Message.CreateWithAttachments(
            _conversationId,
            _senderId,
            "",
            MessageType.Image,
            attachments);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().BeEmpty();
    }

    [Fact]
    public void CreateWithAttachments_WithTextType_ShouldFail()
    {
        // Arrange
        var attachment = Attachment.Create("test.pdf", "https://storage/test.pdf", 1024, "application/pdf").Value;
        var attachments = new List<Attachment> { attachment };

        // Act
        var result = Message.CreateWithAttachments(
            _conversationId,
            _senderId,
            "Text",
            MessageType.Text,
            attachments);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidType");
    }

    [Fact]
    public void CreateWithAttachments_WithSystemType_ShouldFail()
    {
        // Arrange
        var attachment = Attachment.Create("test.pdf", "https://storage/test.pdf", 1024, "application/pdf").Value;
        var attachments = new List<Attachment> { attachment };

        // Act
        var result = Message.CreateWithAttachments(
            _conversationId,
            _senderId,
            "System",
            MessageType.System,
            attachments);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidType");
    }

    [Fact]
    public void CreateWithAttachments_WithNoAttachments_ShouldFail()
    {
        // Act
        var result = Message.CreateWithAttachments(
            _conversationId,
            _senderId,
            "File",
            MessageType.File,
            new List<Attachment>());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.MissingAttachments");
    }

    [Fact]
    public void CreateWithAttachments_WithTooManyAttachments_ShouldFail()
    {
        // Arrange
        var attachments = Enumerable.Range(1, 11)
            .Select(i => Attachment.Create($"file{i}.pdf", $"https://storage/file{i}.pdf", 1024, "application/pdf").Value)
            .ToList();

        // Act
        var result = Message.CreateWithAttachments(
            _conversationId,
            _senderId,
            "Files",
            MessageType.File,
            attachments);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.TooManyAttachments");
    }

    #endregion

    #region CreateSystem Tests

    [Fact]
    public void CreateSystem_WithValidContent_ShouldSucceed()
    {
        // Act
        var result = Message.CreateSystem(_conversationId, "User joined the conversation");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var message = result.Value;
        message.Type.Should().Be(MessageType.System);
        message.SenderId.Should().Be(Guid.Empty);
        message.Content.Should().Be("User joined the conversation");
    }

    [Fact]
    public void CreateSystem_WithNullConversationId_ShouldFail()
    {
        // Act
        var result = Message.CreateSystem(null!, "System message");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidConversation");
    }

    [Fact]
    public void CreateSystem_WithEmptyContent_ShouldFail()
    {
        // Act
        var result = Message.CreateSystem(_conversationId, "   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.EmptyContent");
    }

    #endregion

    #region Edit Tests

    [Fact]
    public void Edit_BySender_ShouldSucceed()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Original message").Value;

        // Act
        var result = message.Edit("Edited message", _senderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.Content.Should().Be("Edited message");
        message.EditedAt.Should().NotBeNull();
        message.EditedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Edit_ShouldTrimContent()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Original").Value;

        // Act
        var result = message.Edit("  Edited  ", _senderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.Content.Should().Be("Edited");
    }

    [Fact]
    public void Edit_ByNonSender_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Original").Value;

        // Act
        var result = message.Edit("Hacked", _userId1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.NotSender");
        message.Content.Should().Be("Original");
    }

    [Fact]
    public void Edit_SystemMessage_ShouldFail()
    {
        // Arrange
        var message = Message.CreateSystem(_conversationId, "System message").Value;

        // Act
        var result = message.Edit("Edited", Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.CannotEditSystem");
    }

    [Fact]
    public void Edit_DeletedMessage_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Original").Value;
        message.Delete(_senderId);

        // Act
        var result = message.Edit("Edited", _senderId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.AlreadyDeleted");
    }

    [Fact]
    public void Edit_WithEmptyContent_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Original").Value;

        // Act
        var result = message.Edit("   ", _senderId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.EmptyContent");
    }

    [Fact]
    public void Edit_WithContentTooLong_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Original").Value;
        var longContent = new string('A', 5001);

        // Act
        var result = message.Edit(longContent, _senderId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.ContentTooLong");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public void Delete_BySender_ShouldSucceed()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message to delete").Value;

        // Act
        var result = message.Delete(_senderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.IsDeleted.Should().BeTrue();
        message.DeletedAt.Should().NotBeNull();
        message.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Delete_ByNonSender_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;

        // Act
        var result = message.Delete(_userId1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.NotSender");
        message.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Delete_SystemMessage_ShouldFail()
    {
        // Arrange
        var message = Message.CreateSystem(_conversationId, "System message").Value;

        // Act
        var result = message.Delete(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.CannotDeleteSystem");
    }

    [Fact]
    public void Delete_AlreadyDeleted_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;
        message.Delete(_senderId);

        // Act
        var result = message.Delete(_senderId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.AlreadyDeleted");
    }

    #endregion

    #region AddReaction Tests

    [Fact]
    public void AddReaction_WithValidEmoji_ShouldSucceed()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;

        // Act
        var result = message.AddReaction(_userId1, "👍");

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.Reactions.Should().HaveCount(1);
        message.Reactions.First().UserId.Should().Be(_userId1);
        message.Reactions.First().Emoji.Should().Be("👍");
    }

    [Fact]
    public void AddReaction_MultipleUsersDifferentEmojis_ShouldSucceed()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;

        // Act
        message.AddReaction(_userId1, "👍");
        message.AddReaction(_userId2, "❤️");

        // Assert
        message.Reactions.Should().HaveCount(2);
    }

    [Fact]
    public void AddReaction_SameUserDifferentEmojis_ShouldSucceed()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;

        // Act
        message.AddReaction(_userId1, "👍");
        var result = message.AddReaction(_userId1, "❤️");

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.Reactions.Should().HaveCount(2);
    }

    [Fact]
    public void AddReaction_SameUserSameEmoji_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;
        message.AddReaction(_userId1, "👍");

        // Act
        var result = message.AddReaction(_userId1, "👍");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.AlreadyReacted");
        message.Reactions.Should().HaveCount(1);
    }

    [Fact]
    public void AddReaction_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;

        // Act
        var result = message.AddReaction(Guid.Empty, "👍");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidUser");
    }

    [Fact]
    public void AddReaction_ToDeletedMessage_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;
        message.Delete(_senderId);

        // Act
        var result = message.AddReaction(_userId1, "👍");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.MessageDeleted");
    }

    [Fact]
    public void AddReaction_WithEmptyEmoji_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;

        // Act
        var result = message.AddReaction(_userId1, "   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reaction.InvalidEmoji");
    }

    #endregion

    #region RemoveReaction Tests

    [Fact]
    public void RemoveReaction_ExistingReaction_ShouldSucceed()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;
        message.AddReaction(_userId1, "👍");

        // Act
        var result = message.RemoveReaction(_userId1, "👍");

        // Assert
        result.IsSuccess.Should().BeTrue();
        message.Reactions.Should().BeEmpty();
    }

    [Fact]
    public void RemoveReaction_NonExistingReaction_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;

        // Act
        var result = message.RemoveReaction(_userId1, "👍");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.ReactionNotFound");
    }

    [Fact]
    public void RemoveReaction_WrongUser_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;
        message.AddReaction(_userId1, "👍");

        // Act
        var result = message.RemoveReaction(_userId2, "👍");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.ReactionNotFound");
    }

    [Fact]
    public void RemoveReaction_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var message = Message.CreateText(_conversationId, _senderId, "Message").Value;

        // Act
        var result = message.RemoveReaction(Guid.Empty, "👍");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Message.InvalidUser");
    }

    #endregion
}
