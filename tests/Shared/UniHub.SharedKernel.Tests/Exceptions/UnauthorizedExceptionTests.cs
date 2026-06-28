using FluentAssertions;
using UniHub.SharedKernel.Exceptions;

namespace UniHub.SharedKernel.Tests.Exceptions;

public class UnauthorizedExceptionTests
{
    [Fact]
    public void Constructor_Default_ShouldCreateWithDefaultMessage()
    {
        // Act
        var exception = new UnauthorizedException();

        // Assert
        exception.Message.Should().Be("Unauthorized access.");
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "User does not have permission";

        // Act
        var exception = new UnauthorizedException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldSetInnerException()
    {
        // Arrange
        var message = "Unauthorized";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new UnauthorizedException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}
