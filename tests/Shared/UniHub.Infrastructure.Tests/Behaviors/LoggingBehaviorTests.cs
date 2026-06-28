using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using UniHub.Infrastructure.Behaviors;

namespace UniHub.Infrastructure.Tests.Behaviors;

public class LoggingBehaviorTests
{
    private readonly Mock<ILogger<LoggingBehavior<TestRequest, string>>> _loggerMock;
    private readonly Mock<RequestHandlerDelegate<string>> _nextMock;

    public LoggingBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        _nextMock = new Mock<RequestHandlerDelegate<string>>();
        _nextMock.Setup(x => x()).ReturnsAsync("Response");
    }

    [Fact]
    public async Task Handle_ShouldLogStartAndEnd()
    {
        // Arrange
        var behavior = new LoggingBehavior<TestRequest, string>(_loggerMock.Object);
        var request = new TestRequest { Value = "test" };

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Response");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[START]")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[END]")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldLogError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _nextMock.Setup(x => x()).ThrowsAsync(exception);

        var behavior = new LoggingBehavior<TestRequest, string>(_loggerMock.Object);
        var request = new TestRequest { Value = "test" };

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("[ERROR]")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    public class TestRequest
    {
        public string? Value { get; set; }
    }
}
