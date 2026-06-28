using FluentAssertions;
using UniHub.Learning.Application.Commands.ApprovalWorkflow;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.ApprovalWorkflow;

public class RejectDocumentCommandValidatorTests
{
    private readonly RejectDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new RejectDocumentCommand(
            DocumentId: Guid.NewGuid(),
            RejectorId: Guid.NewGuid(),
            Reason: "Document does not meet quality standards");

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
        var command = new RejectDocumentCommand(
            DocumentId: Guid.Empty,
            RejectorId: Guid.NewGuid(),
            Reason: "Valid reason");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DocumentId");
    }

    [Fact]
    public void Validate_WithEmptyRejectorId_ShouldFail()
    {
        // Arrange
        var command = new RejectDocumentCommand(
            DocumentId: Guid.NewGuid(),
            RejectorId: Guid.Empty,
            Reason: "Valid reason");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RejectorId");
    }

    [Fact]
    public void Validate_WithEmptyReason_ShouldFail()
    {
        // Arrange
        var command = new RejectDocumentCommand(
            DocumentId: Guid.NewGuid(),
            RejectorId: Guid.NewGuid(),
            Reason: string.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Reason");
    }

    [Fact]
    public void Validate_WithReasonTooShort_ShouldFail()
    {
        // Arrange
        var command = new RejectDocumentCommand(
            DocumentId: Guid.NewGuid(),
            RejectorId: Guid.NewGuid(),
            Reason: "Short");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Reason");
    }

    [Fact]
    public void Validate_WithReasonTooLong_ShouldFail()
    {
        // Arrange
        var command = new RejectDocumentCommand(
            DocumentId: Guid.NewGuid(),
            RejectorId: Guid.NewGuid(),
            Reason: new string('a', 1001));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Reason");
    }
}
