using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.Forum.Application.Queries.GetTags;
using Xunit;

namespace UniHub.Forum.Application.Tests.Queries.GetTags;

public sealed class GetTagsQueryHandlerTests
{
    private readonly ITagRepository _tagRepository;
    private readonly GetTagsQueryHandler _handler;

    public GetTagsQueryHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new GetTagsQueryHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnTags()
    {
        // Arrange
        var query = new GetTagsQuery(PageNumber: 1, PageSize: 20);
        var expectedResult = new GetTagsResult
        {
            Tags = new List<TagDto>
            {
                new() { Id = 1, Name = "csharp", Slug = "csharp", UsageCount = 10 },
                new() { Id = 2, Name = "dotnet", Slug = "dotnet", UsageCount = 5 }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 20
        };

        _tagRepository.GetTagsAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchTerm,
            query.OrderByUsage,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Handle_WithInvalidPageNumber_ShouldReturnFailure(int invalidPageNumber)
    {
        // Arrange
        var query = new GetTagsQuery(PageNumber: invalidPageNumber, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagErrors.InvalidPageNumber);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task Handle_WithInvalidPageSize_ShouldReturnFailure(int invalidPageSize)
    {
        // Arrange
        var query = new GetTagsQuery(PageNumber: 1, PageSize: invalidPageSize);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagErrors.InvalidPageSize);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldPassToRepository()
    {
        // Arrange
        var query = new GetTagsQuery(PageNumber: 1, PageSize: 20, SearchTerm: "csharp");
        var expectedResult = new GetTagsResult
        {
            Tags = new List<TagDto> { new() { Id = 1, Name = "csharp" } },
            TotalCount = 1,
            PageNumber = 1,
            PageSize = 20
        };

        _tagRepository.GetTagsAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            query.SearchTerm,
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tagRepository.Received(1).GetTagsAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchTerm,
            query.OrderByUsage,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithOrderByUsage_ShouldPassToRepository()
    {
        // Arrange
        var query = new GetTagsQuery(PageNumber: 1, PageSize: 20, OrderByUsage: true);
        var expectedResult = new GetTagsResult
        {
            Tags = new List<TagDto>(),
            TotalCount = 0,
            PageNumber = 1,
            PageSize = 20
        };

        _tagRepository.GetTagsAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            true,
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tagRepository.Received(1).GetTagsAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchTerm,
            true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldSucceed()
    {
        // Arrange
        var query = new GetTagsQuery(PageNumber: 5, PageSize: 20);
        var expectedResult = new GetTagsResult
        {
            Tags = new List<TagDto>(),
            TotalCount = 0,
            PageNumber = 5,
            PageSize = 20
        };

        _tagRepository.GetTagsAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<string?>(),
            Arg.Any<bool>(),
            Arg.Any<CancellationToken>())
            .Returns(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Tags.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
