using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using UniHub.Infrastructure.Behaviors;

namespace UniHub.Infrastructure.Tests.Behaviors;

public class UnhandledExceptionBehaviorTests
{
    private readonly Mock<ILogger<UnhandledExceptionBehavior<TestRequest, string>>> _loggerMock;
    private readonly Mock<RequestHandlerDelegate<string>> _nextMock;

    public UnhandledExceptionBehaviorTests()
    {
        _loggerMock = new Mock<ILogger<UnhandledExceptionBehavior<TestRequest, string>>>();
        _nextMock = new Mock<RequestHandlerDelegate<string>>();
    }

    [Fact]
    public async Task Handle_WhenNoException_ShouldCallNextAndReturnResponse()
    {
        // Arrange
        _nextMock.Setup(x => x()).ReturnsAsync("Response");
        var behavior = new UnhandledExceptionBehavior<TestRequest, string>(_loggerMock.Object);
        var request = new TestRequest { Value = "test" };

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Response");
        _nextMock.Verify(x => x(), Times.Once);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExceptionThrown_ShouldLogErrorAndRethrow()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        _nextMock.Setup(x => x()).ThrowsAsync(exception);

        var behavior = new UnhandledExceptionBehavior<TestRequest, string>(_loggerMock.Object);
        var request = new TestRequest { Value = "test" };

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception");

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unhandled Exception")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDifferentExceptionTypes_ShouldLogAndRethrow()
    {
        // Arrange
        var exceptions = new Exception[]
        {
            new ArgumentNullException("param"),
            new InvalidOperationException("Invalid operation"),
            new Exception("Generic exception")
        };

        foreach (var exception in exceptions)
        {
            // Arrange
            var nextMock = new Mock<RequestHandlerDelegate<string>>();
            nextMock.Setup(x => x()).ThrowsAsync(exception);
            var loggerMock = new Mock<ILogger<UnhandledExceptionBehavior<TestRequest, string>>>();

            var behavior = new UnhandledExceptionBehavior<TestRequest, string>(loggerMock.Object);
            var request = new TestRequest { Value = "test" };

            // Act
            var act = async () => await behavior.Handle(request, nextMock.Object, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>();
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }

    public class TestRequest
    {
        public string? Value { get; set; }
    }
}
