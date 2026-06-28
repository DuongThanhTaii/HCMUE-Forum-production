using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.BookmarkPost;
using UniHub.Forum.Application.Commands.UnbookmarkPost;
using UniHub.Forum.Domain.Bookmarks;
using UniHub.Forum.Domain.Posts;
using Xunit;

namespace UniHub.Forum.Application.Tests.Commands.UnbookmarkPost;

public sealed class UnbookmarkPostCommandHandlerTests
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly UnbookmarkPostCommandHandler _handler;

    public UnbookmarkPostCommandHandlerTests()
    {
        _bookmarkRepository = Substitute.For<IBookmarkRepository>();
        _handler = new UnbookmarkPostCommandHandler(_bookmarkRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRemoveBookmark()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new UnbookmarkPostCommand(postId, userId);

        var bookmark = Bookmark.Create(new PostId(postId), userId);
        _bookmarkRepository.GetByUserAndPostAsync(userId, new PostId(postId), Arg.Any<CancellationToken>())
            .Returns(bookmark);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _bookmarkRepository.Received(1).RemoveAsync(bookmark, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentBookmark_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new UnbookmarkPostCommand(postId, userId);

        _bookmarkRepository.GetByUserAndPostAsync(userId, new PostId(postId), Arg.Any<CancellationToken>())
            .Returns((Bookmark?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookmarkErrors.BookmarkNotFound);
        await _bookmarkRepository.DidNotReceive().RemoveAsync(
            Arg.Any<Bookmark>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyPostId_ShouldReturnFailure()
    {
        // Arrange
        var command = new UnbookmarkPostCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var command = new UnbookmarkPostCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WrongUserUnbookmarking_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var command = new UnbookmarkPostCommand(postId, otherUserId);

        // No bookmark exists for otherUserId
        _bookmarkRepository.GetByUserAndPostAsync(otherUserId, new PostId(postId), Arg.Any<CancellationToken>())
            .Returns((Bookmark?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookmarkErrors.BookmarkNotFound);
    }
}
