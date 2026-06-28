using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using UniHub.Infrastructure.Behaviors;

namespace UniHub.Infrastructure.Tests.Behaviors;

public class PerformanceBehaviorTests
{
    private readonly Mock<ILogger<PerformanceBehavior<TestRequest, string>>> _loggerMock;
    private readonly Mock<RequestHandlerDelegate<string>> _nextMock;

    public PerformanceBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<PerformanceBehavior<TestRequest, string>>>();
        _nextMock = new Mock<RequestHandlerDelegate<string>>();
    }

    [Fact]
    public async Task Handle_WhenRequestFast_ShouldNotLogWarning()
    {
        // Arrange
        _nextMock.Setup(x => x()).ReturnsAsync("Response");
        var behavior = new PerformanceBehavior<TestRequest, string>(_loggerMock.Object);
        var request = new TestRequest { Value = "test" };

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Response");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRequestSlow_ShouldLogWarning()
    {
        // Arrange
        _nextMock.Setup(x => x()).Returns(async () =>
        {
            await Task.Delay(600); // Exceed 500ms threshold
            return "Response";
        });

        var behavior = new PerformanceBehavior<TestRequest, string>(_loggerMock.Object);
        var request = new TestRequest { Value = "test" };

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Response");
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Long Running Request")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRequestSlowWithCancellation_ShouldStillComplete()
    {
        // Arrange
        _nextMock.Setup(x => x()).Returns(async () =>
        {
            await Task.Delay(600);
            return "Response";
        });

        var behavior = new PerformanceBehavior<TestRequest, string>(_loggerMock.Object);
        var request = new TestRequest { Value = "test" };
        var cts = new CancellationTokenSource();

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, cts.Token);

        // Assert
        result.Should().Be("Response");
    }

    public class TestRequest
    {
        public string? Value { get; set; }
    }
}
