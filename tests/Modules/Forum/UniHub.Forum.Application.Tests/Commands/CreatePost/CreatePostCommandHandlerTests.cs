using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Application.Tests.Commands.CreatePost;

public sealed class CreatePostCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly CreatePostCommandHandler _handler;

    public CreatePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();
        _handler = new CreatePostCommandHandler(_postRepository, _categoryRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreatePost()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreatePostCommand(
            Title: "Test Post Title",
            Content: "This is a test post content with enough characters.",
            Type: (int)PostType.Discussion,
            AuthorId: authorId,
            CategoryId: null,
            Tags: new[] { "test", "forum" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _postRepository.Received(1).AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCategory_ShouldValidateCategoryExists()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var command = new CreatePostCommand(
            Title: "Test Post Title",
            Content: "This is a test post content with enough characters.",
            Type: (int)PostType.Discussion,
            AuthorId: authorId,
            CategoryId: categoryId,
            Tags: null);

        _categoryRepository.ExistsAsync(Arg.Any<CategoryId>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _categoryRepository.Received(1).ExistsAsync(Arg.Any<CategoryId>(), Arg.Any<CancellationToken>());
        await _postRepository.Received(1).AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidCategory_ShouldReturnFailure()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var command = new CreatePostCommand(
            Title: "Test Post Title",
            Content: "This is a test post content with enough characters.",
            Type: (int)PostType.Discussion,
            AuthorId: authorId,
            CategoryId: categoryId,
            Tags: null);

        _categoryRepository.ExistsAsync(Arg.Any<CategoryId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PostErrors.CategoryNotFound.Code);

        await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidTitle_ShouldReturnFailure()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreatePostCommand(
            Title: "abc", // Too short
            Content: "This is a test post content with enough characters.",
            Type: (int)PostType.Discussion,
            AuthorId: authorId,
            CategoryId: null,
            Tags: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Title");

        await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidContent_ShouldReturnFailure()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreatePostCommand(
            Title: "Test Post Title",
            Content: "Short", // Too short
            Type: (int)PostType.Discussion,
            AuthorId: authorId,
            CategoryId: null,
            Tags: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Content");

        await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidPostType_ShouldReturnFailure()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreatePostCommand(
            Title: "Test Post Title",
            Content: "This is a test post content with enough characters.",
            Type: 999, // Invalid post type
            AuthorId: authorId,
            CategoryId: null,
            Tags: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(PostErrors.InvalidPostType.Code);

        await _postRepository.DidNotReceive().AddAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTags_ShouldCreatePostWithTags()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var tags = new[] { "tag1", "tag2", "tag3" };
        var command = new CreatePostCommand(
            Title: "Test Post Title",
            Content: "This is a test post content with enough characters.",
            Type: (int)PostType.Discussion,
            AuthorId: authorId,
            CategoryId: null,
            Tags: tags);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await _postRepository.Received(1).AddAsync(Arg.Is<Post>(p => p.Tags.Count == tags.Length), Arg.Any<CancellationToken>());
    }
}
