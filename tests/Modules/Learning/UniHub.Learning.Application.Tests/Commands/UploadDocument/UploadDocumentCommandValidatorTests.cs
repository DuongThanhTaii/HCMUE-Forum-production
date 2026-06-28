using FluentAssertions;
using UniHub.Learning.Application.Commands.UploadDocument;
using UniHub.Learning.Domain.Documents;
using Xunit;

namespace UniHub.Learning.Application.Tests.Commands.UploadDocument;

public class UploadDocumentCommandValidatorTests
{
    private readonly UploadDocumentCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Introduction to Computer Science",
            Description: "Course materials for CS101",
            FileName: "lecture01.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide, // Slide
            UploaderId: Guid.NewGuid(),
            CourseId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithEmptyTitle_ShouldFail(string? title)
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: title!,
            Description: "Description",
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public void Validate_WithTitleTooShort_ShouldFail()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "ABC",
            Description: "Description",
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("at least 5"));
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldFail()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: new string('A', 201),
            Description: "Description",
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title" && e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: new string('A', 1001),
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description" && e.ErrorMessage.Contains("1000"));
    }

    [Fact]
    public void Validate_WithEmptyFileName_ShouldFail()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: "Description",
            FileName: "",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileName");
    }

    [Fact]
    public void Validate_WithEmptyFileContent_ShouldFail()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: "Description",
            FileName: "file.pdf",
            FileContent: Array.Empty<byte>(),
            ContentType: "application/pdf",
            FileSize: 0,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileContent" || e.PropertyName == "FileSize");
    }

    [Fact]
    public void Validate_WithFileSizeTooLarge_ShouldFail()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: "Description",
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 51 * 1024 * 1024, // 51MB
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FileSize" && e.ErrorMessage.Contains("50"));
    }

    [Theory]
    [InlineData("text/html")]
    [InlineData("application/javascript")]
    [InlineData("application/x-executable")]
    public void Validate_WithInvalidContentType_ShouldFail(string contentType)
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: "Description",
            FileName: "file.html",
            FileContent: new byte[1024],
            ContentType: contentType,
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ContentType");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("application/vnd.openxmlformats-officedocument.presentationml.presentation")]
    [InlineData("video/mp4")]
    public void Validate_WithValidContentType_ShouldPass(string contentType)
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: "Description",
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: contentType,
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUploaderId_ShouldFail()
    {
        // Arrange
        var command = new UploadDocumentCommand(
            Title: "Valid Title",
            Description: "Description",
            FileName: "file.pdf",
            FileContent: new byte[1024],
            ContentType: "application/pdf",
            FileSize: 1024,
            DocumentType: DocumentType.Slide,
            UploaderId: Guid.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UploaderId");
    }
}
