using FluentAssertions;
using UniHub.SharedKernel.Exceptions;

namespace UniHub.SharedKernel.Tests.Exceptions;

public class ValidationExceptionTests
{
    [Fact]
    public void Constructor_Default_ShouldCreateWithEmptyErrors()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        exception.Message.Should().Be("One or more validation errors occurred.");
        exception.Errors.Should().NotBeNull();
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        // Arrange
        var message = "Custom validation error";

        // Act
        var exception = new ValidationException(message);

        // Assert
        exception.Message.Should().Be(message);
        exception.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithErrors_ShouldSetErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email is invalid" } },
            { "Name", new[] { "Name is required" } }
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        exception.Errors.Should().BeEquivalentTo(errors);
        exception.Message.Should().Be("One or more validation errors occurred.");
    }

    [Fact]
    public void Constructor_WithPropertyNameAndError_ShouldCreateSingleError()
    {
        // Arrange
        var propertyName = "Email";
        var errorMessage = "Email is required";

        // Act
        var exception = new ValidationException(propertyName, errorMessage);

        // Assert
        exception.Errors.Should().ContainKey(propertyName);
        exception.Errors[propertyName].Should().ContainSingle();
        exception.Errors[propertyName].Should().Contain(errorMessage);
    }
}
