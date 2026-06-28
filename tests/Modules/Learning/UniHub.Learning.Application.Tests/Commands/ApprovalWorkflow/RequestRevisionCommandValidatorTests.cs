using FluentAssertions;
using UniHub.Learning.Application.Commands.ApprovalWorkflow;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.ApprovalWorkflow;

public class RequestRevisionCommandValidatorTests
{
    private readonly RequestRevisionCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new RequestRevisionCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            RevisionNotes: "Please add more references");

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
        var command = new RequestRevisionCommand(
            DocumentId: Guid.Empty,
            ReviewerId: Guid.NewGuid(),
            RevisionNotes: "Valid notes");

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
        var command = new RequestRevisionCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.Empty,
            RevisionNotes: "Valid notes");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ReviewerId");
    }

    [Fact]
    public void Validate_WithEmptyRevisionNotes_ShouldFail()
    {
        // Arrange
        var command = new RequestRevisionCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            RevisionNotes: string.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RevisionNotes");
    }

    [Fact]
    public void Validate_WithRevisionNotesTooShort_ShouldFail()
    {
        // Arrange
        var command = new RequestRevisionCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            RevisionNotes: "Short");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RevisionNotes");
    }

    [Fact]
    public void Validate_WithRevisionNotesTooLong_ShouldFail()
    {
        // Arrange
        var command = new RequestRevisionCommand(
            DocumentId: Guid.NewGuid(),
            ReviewerId: Guid.NewGuid(),
            RevisionNotes: new string('a', 1001));

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RevisionNotes");
    }
}
