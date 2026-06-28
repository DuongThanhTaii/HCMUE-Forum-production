using FluentAssertions;
using UniHub.Learning.Application.Commands.DocumentDownload;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.DocumentDownload;

public class DownloadDocumentCommandValidatorTests
{
    private readonly DownloadDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyDocumentId_ShouldFail()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.Empty,
            UserId: Guid.NewGuid());

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
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.NewGuid(),
            UserId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle()
            .Which.ErrorMessage.Should().Be("User ID is required");
    }

    [Fact]
    public void Validate_WithBothIdsEmpty_ShouldFailWithTwoErrors()
    {
        // Arrange
        var command = new DownloadDocumentCommand(
            DocumentId: Guid.Empty,
            UserId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Select(e => e.ErrorMessage).Should().Contain(
            "Document ID is required",
            "User ID is required");
    }
}
