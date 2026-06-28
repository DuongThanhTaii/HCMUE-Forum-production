using FluentAssertions;
using UniHub.Learning.Application.Commands.DocumentRating;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.DocumentRating;

public class RateDocumentCommandValidatorTests
{
    private readonly RateDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RateDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Rating: 4);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyDocumentId_ShouldFail()
    {
        // Arrange
        var command = new RateDocumentCommand(
            DocumentId: Guid.Empty,
            UserId: Guid.NewGuid(),
            Rating: 4);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Document ID is required");
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var command = new RateDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.Empty,
            Rating: 4);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("User ID is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(10)]
    public void Validate_WithInvalidRating_ShouldFail(int rating)
    {
        // Arrange
        var command = new RateDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Rating: rating);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("Rating must be between 1 and 5");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Validate_WithValidRatings_ShouldPass(int rating)
    {
        // Arrange
        var command = new RateDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Rating: rating);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
