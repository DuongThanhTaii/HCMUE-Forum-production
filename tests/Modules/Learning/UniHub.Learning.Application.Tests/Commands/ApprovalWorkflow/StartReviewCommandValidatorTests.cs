using FluentAssertions;
using UniHub.Learning.Application.Commands.ApprovalWorkflow;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.ApprovalWorkflow;

public class StartReviewCommandValidatorTests
{
    private readonly StartReviewCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new StartReviewCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithEmptyDocumentId_ShouldFail()
    {
        // Arrange
        var command = new StartReviewCommand(
            DocumentId: Guid.Empty,
            ReviewerId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DocumentId");
    }

    [Fact]
    public void Validate_WithEmptyReviewerId_ShouldFail()
    {
        // Arrange
        var command = new StartReviewCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ReviewerId");
    }
}
