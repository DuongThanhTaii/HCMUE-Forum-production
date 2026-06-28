using FluentAssertions;
using UniHub.SharedKernel.Exceptions;

namespace UniHub.SharedKernel.Tests.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Entity not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.EntityName.Should().BeNull();
        exception.Key.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEntityNameAndKey_ShouldSetProperties()
    {
        // Arrange
        var entityName = "User";
        var key = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(entityName, key);

        // Assert
        exception.EntityName.Should().Be(entityName);
        exception.Key.Should().Be(key);
        exception.Message.Should().Contain(entityName);
        exception.Message.Should().Contain(key.ToString());
    }

    [Fact]
    public void Constructor_Default_ShouldCreateException()
    {
        // Act
        var exception = new NotFoundException();

        // Assert
        exception.Should().NotBeNull();
        exception.Message.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithInnerException_ShouldSetInnerException()
    {
        // Arrange
        var message = "Not found";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new NotFoundException(message, innerException);

        // Assert
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }
}
