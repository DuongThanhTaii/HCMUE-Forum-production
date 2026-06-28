using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.BookmarkPost;
using UniHub.Forum.Domain.Bookmarks;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;
using Xunit;

namespace UniHub.Forum.Application.Tests.Commands.BookmarkPost;

public sealed class BookmarkPostCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly BookmarkPostCommandHandler _handler;

    public BookmarkPostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _bookmarkRepository = Substitute.For<IBookmarkRepository>();
        _handler = new BookmarkPostCommandHandler(_postRepository, _bookmarkRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldBookmarkPost()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new BookmarkPostCommand(postId, userId);

        var post = Post.Create(
            PostTitle.Create("Test Post").Value,
            PostContent.Create("Test content").Value,
            PostType.Discussion,
            userId,
            null,
            new List<string>()).Value;

        _postRepository.GetByIdAsync(new PostId(postId), Arg.Any<CancellationToken>())
            .Returns(post);
        _bookmarkRepository.GetByUserAndPostAsync(userId, new PostId(postId), Arg.Any<CancellationToken>())
            .Returns((Bookmark?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _bookmarkRepository.Received(1).AddAsync(
            Arg.Is<Bookmark>(b => b.PostId.Value == postId && b.UserId == userId),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new BookmarkPostCommand(postId, userId);

        _postRepository.GetByIdAsync(new PostId(postId), Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookmarkErrors.PostNotFound);
        await _bookmarkRepository.DidNotReceive().AddAsync(
            Arg.Any<Bookmark>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithAlreadyBookmarkedPost_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new BookmarkPostCommand(postId, userId);

        var post = Post.Create(
            PostTitle.Create("Test Post").Value,
            PostContent.Create("Test content").Value,
            PostType.Discussion,
            userId,
            null,
            new List<string>()).Value;

        var existingBookmark = Bookmark.Create(new PostId(postId), userId);

        _postRepository.GetByIdAsync(new PostId(postId), Arg.Any<CancellationToken>())
            .Returns(post);
        _bookmarkRepository.GetByUserAndPostAsync(userId, new PostId(postId), Arg.Any<CancellationToken>())
            .Returns(existingBookmark);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookmarkErrors.AlreadyBookmarked);
        await _bookmarkRepository.DidNotReceive().AddAsync(
            Arg.Any<Bookmark>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyPostId_ShouldReturnFailure()
    {
        // Arrange
        var command = new BookmarkPostCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var command = new BookmarkPostCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_MultipleUsersBookmarkingSamePost_ShouldSucceed()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        var post = Post.Create(
            PostTitle.Create("Test Post").Value,
            PostContent.Create("Test content").Value,
            PostType.Discussion,
            userId1,
            null,
            new List<string>()).Value;

        _postRepository.GetByIdAsync(new PostId(postId), Arg.Any<CancellationToken>())
            .Returns(post);

        // User 1 bookmarks
        var command1 = new BookmarkPostCommand(postId, userId1);
        _bookmarkRepository.GetByUserAndPostAsync(userId1, new PostId(postId), Arg.Any<CancellationToken>())
            .Returns((Bookmark?)null);

        var result1 = await _handler.Handle(command1, CancellationToken.None);

        // User 2 bookmarks
        var command2 = new BookmarkPostCommand(postId, userId2);
        _bookmarkRepository.GetByUserAndPostAsync(userId2, new PostId(postId), Arg.Any<CancellationToken>())
            .Returns((Bookmark?)null);

        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        await _bookmarkRepository.Received(2).AddAsync(
            Arg.Any<Bookmark>(),
            Arg.Any<CancellationToken>());
    }
}
