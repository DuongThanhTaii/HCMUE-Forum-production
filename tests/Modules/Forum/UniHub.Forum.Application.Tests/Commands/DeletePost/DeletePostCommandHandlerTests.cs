using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Application.Commands.DeletePost;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;

namespace UniHub.Forum.Application.Tests.Commands.DeletePost;

public sealed class DeletePostCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly DeletePostCommandHandler _handler;

    public DeletePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new DeletePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeletePost()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var post = CreatePost(postId, authorId);

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new DeletePostCommand(postId, authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Status.Should().Be(PostStatus.Deleted);

        await _postRepository.Received(1).UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeletePostCommand(Guid.NewGuid(), Guid.NewGuid());

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PostErrors.PostNotFound.Code);

        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailure()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var unauthorizedUserId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var post = CreatePost(postId, authorId);

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new DeletePostCommand(postId, unauthorizedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PostErrors.UnauthorizedAccess.Code);

        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyDeletedPost_ShouldReturnFailure()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var post = CreatePost(postId, authorId);
        post.Delete(); // Already deleted

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new DeletePostCommand(postId, authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    private static Post CreatePost(Guid postId, Guid authorId)
    {
        var title = PostTitle.Create("Test Title").Value;
        var content = PostContent.Create("Test content with enough characters.").Value;
        var post = Post.Create(title, content, PostType.Discussion, authorId).Value;

        // Use reflection to set the ID
        typeof(Post).GetProperty("Id")!.SetValue(post, new PostId(postId));

        return post;
    }
}
