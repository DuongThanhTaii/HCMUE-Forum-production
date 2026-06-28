using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Application.Commands.DeleteComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Application.Tests.Commands.DeleteComment;

public class DeleteCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly DeleteCommentCommandHandler _handler;

    public DeleteCommentCommandHandlerTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _handler = new DeleteCommentCommandHandler(_commentRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteComment()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var comment = CreateTestComment(commentId, authorId);
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new DeleteCommentCommand(commentId, authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.IsDeleted.Should().BeTrue();
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentComment_ShouldReturnFailure()
    {
        // Arrange
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var command = new DeleteCommentCommand(Guid.NewGuid(), Guid.NewGuid());

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

        var command = new DeleteCommentCommand(commentId, unauthorizedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.UnauthorizedAccess);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyDeletedComment_ShouldReturnFailure()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var comment = CreateTestComment(commentId, authorId);
        comment.Delete(); // Already deleted

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new DeleteCommentCommand(commentId, authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.AlreadyDeleted");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    private static Comment CreateTestComment(Guid commentId, Guid authorId)
    {
        var content = CommentContent.Create("Test content").Value;
        var comment = Comment.Create(new PostId(Guid.NewGuid()), authorId, content, null).Value;
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, new CommentId(commentId));
        return comment;
    }
}
