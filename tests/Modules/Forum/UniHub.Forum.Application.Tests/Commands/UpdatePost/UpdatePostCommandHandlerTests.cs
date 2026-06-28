using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Application.Commands.UpdatePost;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;

namespace UniHub.Forum.Application.Tests.Commands.UpdatePost;

public sealed class UpdatePostCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly UpdatePostCommandHandler _handler;

    public UpdatePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _handler = new UpdatePostCommandHandler(_postRepository, _categoryRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdatePost()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var post = CreatePost(postId, authorId);

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new UpdatePostCommand(
            PostId: postId,
            Title: "Updated Title",
            Content: "Updated content with enough characters.",
            CategoryId: null,
            Tags: new[] { "updated" },
            RequestingUserId: authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _postRepository.Received(1).UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdatePostCommand(
            PostId: Guid.NewGuid(),
            Title: "Updated Title",
            Content: "Updated content with enough characters.",
            CategoryId: null,
            Tags: null,
            RequestingUserId: Guid.NewGuid());

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

        var command = new UpdatePostCommand(
            PostId: postId,
            Title: "Updated Title",
            Content: "Updated content with enough characters.",
            CategoryId: null,
            Tags: null,
            RequestingUserId: unauthorizedUserId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PostErrors.UnauthorizedAccess.Code);

        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCategory_ShouldReturnFailure()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var post = CreatePost(postId, authorId);

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _categoryRepository.ExistsAsync(Arg.Any<CategoryId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdatePostCommand(
            PostId: postId,
            Title: "Updated Title",
            Content: "Updated content with enough characters.",
            CategoryId: categoryId,
            Tags: null,
            RequestingUserId: authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PostErrors.CategoryNotFound.Code);

        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithValidCategory_ShouldUpdatePost()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var post = CreatePost(postId, authorId);

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _categoryRepository.ExistsAsync(Arg.Any<CategoryId>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdatePostCommand(
            PostId: postId,
            Title: "Updated Title",
            Content: "Updated content with enough characters.",
            CategoryId: categoryId,
            Tags: null,
            RequestingUserId: authorId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _categoryRepository.Received(1).ExistsAsync(Arg.Any<CategoryId>(), Arg.Any<CancellationToken>());
        await _postRepository.Received(1).UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    private static Post CreatePost(Guid postId, Guid authorId)
    {
        var title = PostTitle.Create("Original Title").Value;
        var content = PostContent.Create("Original content with enough characters.").Value;
        var post = Post.Create(title, content, PostType.Discussion, authorId).Value;

        // Use reflection to set the ID
        typeof(Post).GetProperty("Id")!.SetValue(post, new PostId(postId));

        return post;
    }
}
