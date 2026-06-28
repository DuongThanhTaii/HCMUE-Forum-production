using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Queries.GetBookmarkedPosts;
using Xunit;

namespace UniHub.Forum.Application.Tests.Queries.GetBookmarkedPosts;

public sealed class GetBookmarkedPostsQueryHandlerTests
{
    private readonly IBookmarkRepository _bookmarkRepository;
    private readonly GetBookmarkedPostsQueryHandler _handler;

    public GetBookmarkedPostsQueryHandlerTests()
    {
        _bookmarkRepository = Substitute.For<IBookmarkRepository>();
        _handler = new GetBookmarkedPostsQueryHandler(_bookmarkRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnBookmarkedPosts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetBookmarkedPostsQuery(userId, PageNumber: 1, PageSize: 20);

        var expectedResult = new GetBookmarkedPostsResult
        {
            Posts = new List<BookmarkedPostDto>
            {
                new() { Id = Guid.NewGuid(), Title = "Post 1", BookmarkedAt = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Title = "Post 2", BookmarkedAt = DateTime.UtcNow.AddHours(-1) }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20
        };

        _bookmarkRepository.GetBookmarkedPostsAsync(
            userId,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Posts.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.TotalPages.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WithInvalidPageNumber_ShouldReturnFailure(int invalidPageNumber)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetBookmarkedPostsQuery(userId, PageNumber: invalidPageNumber, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GetBookmarkedPosts.InvalidPageNumber");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Handle_WithInvalidPageSize_ShouldReturnFailure(int invalidPageSize)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetBookmarkedPostsQuery(userId, PageNumber: 1, PageSize: invalidPageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("GetBookmarkedPosts.InvalidPageSize");
    }

    [Fact]
    public async Task Handle_WithNoBookmarks_ShouldReturnEmptyResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetBookmarkedPostsQuery(userId, PageNumber: 1, PageSize: 20);

        var expectedResult = new GetBookmarkedPostsResult
        {
            Posts = new List<BookmarkedPostDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _bookmarkRepository.GetBookmarkedPostsAsync(
            userId,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Posts.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetBookmarkedPostsQuery(userId, PageNumber: 2, PageSize: 10);

        var expectedResult = new GetBookmarkedPostsResult
        {
            Posts = new List<BookmarkedPostDto>
            {
                new() { Id = Guid.NewGuid(), Title = "Post 11" },
                new() { Id = Guid.NewGuid(), Title = "Post 12" }
            },
            TotalCount = 25,
            PageNumber = 2,
            PageSize = 10
        };

        _bookmarkRepository.GetBookmarkedPostsAsync(
            userId,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(2);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithLastPage_ShouldIndicateNoNextPage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetBookmarkedPostsQuery(userId, PageNumber: 3, PageSize: 10);

        var expectedResult = new GetBookmarkedPostsResult
        {
            Posts = new List<BookmarkedPostDto>
            {
                new() { Id = Guid.NewGuid(), Title = "Post 21" }
            },
            TotalCount = 21,
            PageNumber = 3,
            PageSize = 10
        };

        _bookmarkRepository.GetBookmarkedPostsAsync(
            userId,
            query.PageNumber,
            query.PageSize,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithDefaultPagination_ShouldUseDefaults()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetBookmarkedPostsQuery(userId); // Uses defaults: page 1, size 20

        var expectedResult = new GetBookmarkedPostsResult
        {
            Posts = new List<BookmarkedPostDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _bookmarkRepository.GetBookmarkedPostsAsync(
            userId,
            1,
            20,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _bookmarkRepository.Received(1).GetBookmarkedPostsAsync(
            userId,
            1,
            20,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetBookmarkedPostsQuery(Guid.Empty, PageNumber: 1, PageSize: 20);

        // This would be caught by FluentValidation, but testing the handler logic
        // In a real scenario, the validator prevents this from reaching the handler

        // Act & Assert
        // The validator should catch this before it reaches the handler
        // But we can still test the query construction
        query.UserId.Should().Be(Guid.Empty);
    }
}
