using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.Forum.Application.Queries.GetPopularTags;
using Xunit;

namespace UniHub.Forum.Application.Tests.Queries.GetPopularTags;

public sealed class GetPopularTagsQueryHandlerTests
{
    private readonly ITagRepository _tagRepository;
    private readonly GetPopularTagsQueryHandler _handler;

    public GetPopularTagsQueryHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new GetPopularTagsQueryHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPopularTags()
    {
        // Arrange
        var query = new GetPopularTagsQuery(Count: 10);
        var expectedTags = new List<PopularTagDto>
        {
            new() { Id = 1, Name = "csharp", Slug = "csharp", UsageCount = 100 },
            new() { Id = 2, Name = "dotnet", Slug = "dotnet", UsageCount = 80 },
            new() { Id = 3, Name = "aspnet", Slug = "aspnet", UsageCount = 60 }
        };

        _tagRepository.GetPopularTagsAsync(query.Count, Arg.Any<CancellationToken>())
            .Returns(expectedTags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value.Should().BeInDescendingOrder(t => t.UsageCount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(51)]
    public async Task Handle_WithInvalidCount_ShouldReturnFailure(int invalidCount)
    {
        // Arrange
        var query = new GetPopularTagsQuery(Count: invalidCount);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagErrors.InvalidCount);
    }

    [Fact]
    public async Task Handle_WithDefaultCount_ShouldReturnTenTags()
    {
        // Arrange
        var query = new GetPopularTagsQuery(); // Default count = 10
        var expectedTags = Enumerable.Range(1, 10)
            .Select(i => new PopularTagDto { Id = i, Name = $"tag{i}", UsageCount = 100 - i })
            .ToList();

        _tagRepository.GetPopularTagsAsync(10, Arg.Any<CancellationToken>())
            .Returns(expectedTags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_WithFewerTagsThanRequested_ShouldReturnAvailableTags()
    {
        // Arrange
        var query = new GetPopularTagsQuery(Count: 20);
        var expectedTags = new List<PopularTagDto>
        {
            new() { Id = 1, Name = "csharp", UsageCount = 50 },
            new() { Id = 2, Name = "dotnet", UsageCount = 30 }
        };

        _tagRepository.GetPopularTagsAsync(query.Count, Arg.Any<CancellationToken>())
            .Returns(expectedTags);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithNoTags_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetPopularTagsQuery(Count: 10);
        _tagRepository.GetPopularTagsAsync(query.Count, Arg.Any<CancellationToken>())
            .Returns(new List<PopularTagDto>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
