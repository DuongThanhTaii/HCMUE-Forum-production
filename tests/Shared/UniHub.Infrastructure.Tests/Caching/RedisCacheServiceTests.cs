using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using StackExchange.Redis;
using UniHub.Infrastructure.Caching;

namespace UniHub.Infrastructure.Tests.Caching;

public class RedisCacheServiceTests
{
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly Mock<IConnectionMultiplexer> _connectionMultiplexerMock;
    private readonly RedisCacheService _cacheService;

    public RedisCacheServiceTests()
    {
        _cacheMock = new Mock<IDistributedCache>();
        _connectionMultiplexerMock = new Mock<IConnectionMultiplexer>();
        _cacheService = new RedisCacheService(_cacheMock.Object, _connectionMultiplexerMock.Object);
    }

    [Fact]
    public async Task GetAsync_WhenValueExists_ShouldReturnValue()
    {
        // Arrange
        var key = "test-key";
        var testData = new TestData { Id = 1, Name = "Test" };
        var serializedValue = System.Text.Json.JsonSerializer.Serialize(testData);
        var bytes = System.Text.Encoding.UTF8.GetBytes(serializedValue);
        
        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_WhenValueDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var key = "non-existent-key";
        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_ShouldSerializeAndCacheValue()
    {
        // Arrange
        var key = "test-key";
        var testData = new TestData { Id = 1, Name = "Test" };

        // Act
        await _cacheService.SetAsync(key, testData);

        // Assert
        _cacheMock.Verify(
            x => x.SetAsync(
                key,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task SetAsync_WithExpiration_ShouldUseProvidedExpiration()
    {
        // Arrange
        var key = "test-key";
        var testData = new TestData { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMinutes(30);

        // Act
        await _cacheService.SetAsync(key, testData, expiration);

        // Assert
        _cacheMock.Verify(
            x => x.SetAsync(
                key,
                It.IsAny<byte[]>(),
                It.Is<DistributedCacheEntryOptions>(o => 
                    o.AbsoluteExpirationRelativeToNow == expiration),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RemoveAsync_ShouldCallCacheRemove()
    {
        // Arrange
        var key = "test-key";

        // Act
        await _cacheService.RemoveAsync(key);

        // Assert
        _cacheMock.Verify(
            x => x.RemoveAsync(key, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrSetAsync_WhenValueExists_ShouldReturnCachedValue()
    {
        // Arrange
        var key = "test-key";
        var cachedData = new TestData { Id = 1, Name = "Cached" };
        var serializedValue = System.Text.Json.JsonSerializer.Serialize(cachedData);
        var bytes = System.Text.Encoding.UTF8.GetBytes(serializedValue);
        
        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        var factoryCalled = false;
        var factory = new Func<Task<TestData>>(() =>
        {
            factoryCalled = true;
            return Task.FromResult(new TestData { Id = 2, Name = "New" });
        });

        // Act
        var result = await _cacheService.GetOrSetAsync(key, factory);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Cached");
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WhenValueDoesNotExist_ShouldCallFactoryAndCacheResult()
    {
        // Arrange
        var key = "test-key";
        _cacheMock.Setup(x => x.GetAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        var newData = new TestData { Id = 2, Name = "New" };
        var factory = new Func<Task<TestData>>(() => Task.FromResult(newData));

        // Act
        var result = await _cacheService.GetOrSetAsync(key, factory);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Name.Should().Be("New");
        
        _cacheMock.Verify(
            x => x.SetAsync(
                key,
                It.IsAny<byte[]>(),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public void Constructor_WhenCacheIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new RedisCacheService(null!, _connectionMultiplexerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("cache");
    }

    [Fact]
    public void Constructor_WhenConnectionMultiplexerIsNull_ShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new RedisCacheService(_cacheMock.Object, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("connectionMultiplexer");
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
