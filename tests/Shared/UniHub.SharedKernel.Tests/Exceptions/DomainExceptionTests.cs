using FluentAssertions;
using UniHub.SharedKernel.Exceptions;

namespace UniHub.SharedKernel.Tests.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Domain rule violated";

        // Act
        var exception = new DomainException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Name.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithNameAndMessage_ShouldSetBothProperties()
    {
        // Arrange
        var name = "UserMustBeActive";
        var message = "User account is not active";

        // Act
        var exception = new DomainException(name, message);

        // Assert
        exception.Name.Should().Be(name);
        exception.Message.Should().Contain(name);
        exception.Message.Should().Contain(message);
    }

    [Fact]
    public void Constructor_Default_ShouldCreateException()
    {
        // Act
        var exception = new DomainException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldSetInnerException()
    {
        // Arrange
        var message = "Domain error";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new DomainException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}
