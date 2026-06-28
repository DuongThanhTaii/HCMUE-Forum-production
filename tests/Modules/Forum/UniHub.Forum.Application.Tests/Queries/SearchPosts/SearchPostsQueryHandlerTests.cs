using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Queries.SearchPosts;

namespace UniHub.Forum.Application.Tests.Queries.SearchPosts;

public class SearchPostsQueryHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly SearchPostsQueryHandler _handler;

    public SearchPostsQueryHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new SearchPostsQueryHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSearchResults()
    {
        // Arrange
        var searchTerm = "test search";
        var query = new SearchPostsQuery(searchTerm, PageNumber: 1, PageSize: 20);

        var searchResult = new SearchPostsResult
        {
            Posts = new List<PostSearchResult>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Title = "Test Post",
                    Content = "Test content",
                    Slug = "test-post",
                    SearchRank = 0.95
                }
            }.AsReadOnly(),
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };

        _postRepository.SearchAsync(
            searchTerm,
            null,
            null,
            null,
            1,
            20,
            Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Posts.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(1);
        result.Value.Posts.First().SearchRank.Should().Be(0.95);

        await _postRepository.Received(1).SearchAsync(
            searchTerm,
            null,
            null,
            null,
            1,
            20,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ShouldPassCategoryToRepository()
    {
        // Arrange
        var categoryId = 123;
        var query = new SearchPostsQuery("test", CategoryId: categoryId);

        var searchResult = new SearchPostsResult
        {
            Posts = Array.Empty<PostSearchResult>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _postRepository.SearchAsync(
            Arg.Any<string>(),
            categoryId,
            Arg.Any<int?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _postRepository.Received(1).SearchAsync(
            "test",
            categoryId,
            Arg.Any<int?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPostTypeFilter_ShouldPassPostTypeToRepository()
    {
        // Arrange
        var postType = 1; // Discussion
        var query = new SearchPostsQuery("test", PostType: postType);

        var searchResult = new SearchPostsResult
        {
            Posts = Array.Empty<PostSearchResult>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _postRepository.SearchAsync(
            Arg.Any<string>(),
            Arg.Any<int?>(),
            postType,
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _postRepository.Received(1).SearchAsync(
            "test",
            Arg.Any<int?>(),
            postType,
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTagsFilter_ShouldPassTagsToRepository()
    {
        // Arrange
        var tags = new[] { "csharp", "dotnet" };
        var query = new SearchPostsQuery("test", Tags: tags);

        var searchResult = new SearchPostsResult
        {
            Posts = Array.Empty<PostSearchResult>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _postRepository.SearchAsync(
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            tags,
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _postRepository.Received(1).SearchAsync(
            "test",
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            tags,
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptySearchTerm_ShouldReturnFailure()
    {
        // Arrange
        var query = new SearchPostsQuery("");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Search.EmptySearchTerm");
        await _postRepository.DidNotReceive().SearchAsync(
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithWhitespaceSearchTerm_ShouldReturnFailure()
    {
        // Arrange
        var query = new SearchPostsQuery("   ");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Search.EmptySearchTerm");
    }

    [Fact]
    public async Task Handle_WithInvalidPageNumber_ShouldReturnFailure()
    {
        // Arrange
        var query = new SearchPostsQuery("test", PageNumber: 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Search.InvalidPageNumber");
    }

    [Fact]
    public async Task Handle_WithNegativePageNumber_ShouldReturnFailure()
    {
        // Arrange
        var query = new SearchPostsQuery("test", PageNumber: -1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Search.InvalidPageNumber");
    }

    [Fact]
    public async Task Handle_WithPageSizeTooSmall_ShouldReturnFailure()
    {
        // Arrange
        var query = new SearchPostsQuery("test", PageSize: 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Search.InvalidPageSize");
    }

    [Fact]
    public async Task Handle_WithPageSizeTooLarge_ShouldReturnFailure()
    {
        // Arrange
        var query = new SearchPostsQuery("test", PageSize: 101);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Search.InvalidPageSize");
    }

    [Fact]
    public async Task Handle_WithValidPagination_ShouldPassCorrectParameters()
    {
        // Arrange
        var query = new SearchPostsQuery("test", PageNumber: 3, PageSize: 10);

        var searchResult = new SearchPostsResult
        {
            Posts = Array.Empty<PostSearchResult>(),
            TotalCount = 0,
            PageNumber = 3,
            PageSize = 10
        };

        _postRepository.SearchAsync(
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<IEnumerable<string>?>(),
            3,
            10,
            Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(3);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithMultipleResults_ShouldReturnAllResults()
    {
        // Arrange
        var query = new SearchPostsQuery("test");

        var searchResult = new SearchPostsResult
        {
            Posts = new List<PostSearchResult>
            {
                new() { Id = Guid.NewGuid(), Title = "Post 1", SearchRank = 0.95 },
                new() { Id = Guid.NewGuid(), Title = "Post 2", SearchRank = 0.85 },
                new() { Id = Guid.NewGuid(), Title = "Post 3", SearchRank = 0.75 }
            }.AsReadOnly(),
            TotalCount = 3,
            PageNumber = 1,
            PageSize = 20
        };

        _postRepository.SearchAsync(
            Arg.Any<string>(),
            Arg.Any<int?>(),
            Arg.Any<int?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Posts.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.Posts.Should().BeInDescendingOrder(p => p.SearchRank);
    }

    [Fact]
    public async Task Handle_WithAllFilters_ShouldPassAllParametersToRepository()
    {
        // Arrange
        var searchTerm = "test";
        var categoryId = 123;
        var postType = 1;
        var tags = new[] { "csharp", "dotnet" };
        var pageNumber = 2;
        var pageSize = 15;

        var query = new SearchPostsQuery(
            searchTerm,
            categoryId,
            postType,
            tags,
            pageNumber,
            pageSize);

        var searchResult = new SearchPostsResult
        {
            Posts = Array.Empty<PostSearchResult>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _postRepository.SearchAsync(
            searchTerm,
            categoryId,
            postType,
            tags,
            pageNumber,
            pageSize,
            Arg.Any<CancellationToken>())
            .Returns(searchResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _postRepository.Received(1).SearchAsync(
            searchTerm,
            categoryId,
            postType,
            tags,
            pageNumber,
            pageSize,
            Arg.Any<CancellationToken>());
    }
}
