using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Application.Tests.Commands.AddComment;

public class AddCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;
    private readonly AddCommentCommandHandler _handler;

    public AddCommentCommandHandlerTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new AddCommentCommandHandler(_commentRepository, _postRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddComment()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var content = "This is a test comment";

        var post = CreateTestPost(postId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new AddCommentCommand(postId, authorId, content);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        await _commentRepository.Received(1).AddAsync(
            Arg.Is<Comment>(c => 
                c.AuthorId == authorId && 
                c.Content.Value == content &&
                c.PostId.Value == postId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithParentComment_ShouldAddReply()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var parentCommentId = Guid.NewGuid();
        var content = "This is a reply";

        var post = CreateTestPost(postId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var parentComment = CreateTestComment(parentCommentId, postId, authorId);
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(parentComment);

        var command = new AddCommentCommand(postId, authorId, content, parentCommentId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _commentRepository.Received(1).AddAsync(
            Arg.Is<Comment>(c => 
                c.ParentCommentId != null &&
                c.ParentCommentId.Value == parentCommentId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddCommentCommand(Guid.NewGuid(), Guid.NewGuid(), "Test content");
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.PostNotFound);
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentParentComment_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = CreateTestPost(postId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var command = new AddCommentCommand(postId, Guid.NewGuid(), "Test content", Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.ParentCommentNotFound);
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyContent_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = CreateTestPost(postId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new AddCommentCommand(postId, Guid.NewGuid(), "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CommentContent.Empty");
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTooLongContent_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var post = CreateTestPost(postId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var longContent = new string('a', CommentContent.MaxLength + 1);
        var command = new AddCommentCommand(postId, Guid.NewGuid(), longContent);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CommentContent.TooLong");
        await _commentRepository.DidNotReceive().AddAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    private static Post CreateTestPost(Guid postId)
    {
        var post = (Post)Activator.CreateInstance(typeof(Post), true)!;
        typeof(Post).GetProperty(nameof(Post.Id))!.SetValue(post, new PostId(postId));
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
