using FluentAssertions;
using UniHub.Learning.Application.Commands.ApprovalWorkflow;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.ApprovalWorkflow;

public class ApproveDocumentCommandValidatorTests
{
    private readonly ApproveDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new ApproveDocumentCommand(
            DocumentId: Guid.NewGuid(),
            ApproverId: Guid.NewGuid(),
            ApprovalNotes: "Good work!");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithoutApprovalNotes_ShouldPass()
    {
        // Arrange
        var command = new ApproveDocumentCommand(
            DocumentId: Guid.NewGuid(),
            ApproverId: Guid.NewGuid(),
            ApprovalNotes: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyDocumentId_ShouldFail()
    {
        // Arrange
        var command = new ApproveDocumentCommand(
            DocumentId: Guid.Empty,
            ApproverId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DocumentId");
    }

    [Fact]
    public void Validate_WithEmptyApproverId_ShouldFail()
    {
        // Arrange
        var command = new ApproveDocumentCommand(
            DocumentId: Guid.NewGuid(),
            ApproverId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ApproverId");
    }

    [Fact]
    public void Validate_WithApprovalNotesTooLong_ShouldFail()
    {
        // Arrange
        var command = new ApproveDocumentCommand(
            DocumentId: Guid.NewGuid(),
            ApproverId: Guid.NewGuid(),
            ApprovalNotes: new string('a', 1001));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ApprovalNotes");
    }
}
