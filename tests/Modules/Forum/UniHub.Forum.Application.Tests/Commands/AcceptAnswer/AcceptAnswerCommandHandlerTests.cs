using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AcceptAnswer;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;

namespace UniHub.Forum.Application.Tests.Commands.AcceptAnswer;

public class AcceptAnswerCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly AcceptAnswerCommandHandler _handler;

    public AcceptAnswerCommandHandlerTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new AcceptAnswerCommandHandler(_commentRepository, _postRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAcceptAnswer()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var commentAuthorId = Guid.NewGuid();

        var post = CreateTestPost(postId, postAuthorId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var comment = CreateTestComment(commentId, postId, commentAuthorId);
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new AcceptAnswerCommand(commentId, postId, postAuthorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.IsAcceptedAnswer.Should().BeTrue();
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailure()
    {
        // Arrange
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        var command = new AcceptAnswerCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.PostNotFound);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();

        var post = CreateTestPost(postId, postAuthorId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new AcceptAnswerCommand(Guid.NewGuid(), postId, unauthorizedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.UnauthorizedAccess);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentComment_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();

        var post = CreateTestPost(postId, postAuthorId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var command = new AcceptAnswerCommand(Guid.NewGuid(), postId, postAuthorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.CommentNotFound);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCommentFromDifferentPost_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();
        var differentPostId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var post = CreateTestPost(postId, postAuthorId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var comment = CreateTestComment(commentId, differentPostId, Guid.NewGuid());
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new AcceptAnswerCommand(commentId, postId, postAuthorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.WrongPost");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDeletedComment_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var post = CreateTestPost(postId, postAuthorId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var comment = CreateTestComment(commentId, postId, Guid.NewGuid());
        comment.Delete(); // Mark as deleted

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new AcceptAnswerCommand(commentId, postId, postAuthorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyAcceptedComment_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        var post = CreateTestPost(postId, postAuthorId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var comment = CreateTestComment(commentId, postId, Guid.NewGuid());
        comment.AcceptAsAnswer(); // Already accepted

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new AcceptAnswerCommand(commentId, postId, postAuthorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.AlreadyAccepted");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNestedComment_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var postAuthorId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();

        var post = CreateTestPost(postId, postAuthorId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var content = CommentContent.Create("Test reply").Value;
        var comment = Comment.Create(new PostId(postId), Guid.NewGuid(), content, new CommentId(parentCommentId)).Value;
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, new CommentId(commentId));

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new AcceptAnswerCommand(commentId, postId, postAuthorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NestedComment");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    private static Post CreateTestPost(Guid postId, Guid authorId)
    {
        var post = (Post)Activator.CreateInstance(typeof(Post), true)!;
        typeof(Post).GetProperty(nameof(Post.Id))!.SetValue(post, new PostId(postId));
        typeof(Post).GetProperty(nameof(Post.AuthorId))!.SetValue(post, authorId);
        return post;
    }

    private static Comment CreateTestComment(Guid commentId, Guid postId, Guid authorId)
    {
        var content = CommentContent.Create("Test content").Value;
        var comment = Comment.Create(new PostId(postId), authorId, content, null).Value;
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, new CommentId(commentId));
        return comment;
    }
}
