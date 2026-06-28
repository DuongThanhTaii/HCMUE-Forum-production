using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Application.Commands.UpdateComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Application.Tests.Commands.UpdateComment;

public class UpdateCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly UpdateCommentCommandHandler _handler;

    public UpdateCommentCommandHandlerTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _handler = new UpdateCommentCommandHandler(_commentRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var newContent = "Updated content";

        var comment = CreateTestComment(commentId, authorId);
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new UpdateCommentCommand(commentId, newContent, authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Content.Value.Should().Be(newContent);
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentComment_ShouldReturnFailure()
    {
        // Arrange
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var command = new UpdateCommentCommand(Guid.NewGuid(), "New content", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.CommentNotFound);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailure()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();

        var comment = CreateTestComment(commentId, authorId);
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new UpdateCommentCommand(commentId, "New content", unauthorizedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.UnauthorizedAccess);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDeletedComment_ShouldReturnFailure()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var comment = CreateTestComment(commentId, authorId);
        comment.Delete(); // Mark as deleted

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new UpdateCommentCommand(commentId, "New content", authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyContent_ShouldReturnFailure()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var comment = CreateTestComment(commentId, authorId);
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new UpdateCommentCommand(commentId, "", authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CommentContent.Empty");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    private static Comment CreateTestComment(Guid commentId, Guid authorId)
    {
        var content = CommentContent.Create("Original content").Value;
        var comment = Comment.Create(new PostId(Guid.NewGuid()), authorId, content, null).Value;
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, new CommentId(commentId));
        return comment;
    }
}
