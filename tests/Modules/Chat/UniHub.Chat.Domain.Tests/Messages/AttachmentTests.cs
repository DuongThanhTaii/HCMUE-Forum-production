using UniHub.Chat.Domain.Messages;

namespace UniHub.Chat.Domain.Tests.Messages;

public class AttachmentTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Act
        var result = Attachment.Create("document.pdf", "https://storage/doc.pdf", 2048, "application/pdf");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var attachment = result.Value;
        attachment.FileName.Should().Be("document.pdf");
        attachment.FileUrl.Should().Be("https://storage/doc.pdf");
        attachment.FileSizeBytes.Should().Be(2048);
        attachment.MimeType.Should().Be("application/pdf");
        attachment.ThumbnailUrl.Should().BeNull();
    }

    [Fact]
    public void Create_WithThumbnail_ShouldSucceed()
    {
        // Act
        var result = Attachment.Create(
            "photo.jpg",
            "https://storage/photo.jpg",
            1024000,
            "image/jpeg",
            "https://storage/thumb_photo.jpg");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ThumbnailUrl.Should().Be("https://storage/thumb_photo.jpg");
    }

    [Fact]
    public void Create_WithEmptyFileName_ShouldFail()
    {
        // Act
        var result = Attachment.Create("   ", "https://storage/file", 1024, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.InvalidFileName");
    }

    [Fact]
    public void Create_WithFileNameTooLong_ShouldFail()
    {
        // Arrange
        var longFileName = new string('A', 256) + ".pdf";

        // Act
        var result = Attachment.Create(longFileName, "https://storage/file", 1024, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.FileNameTooLong");
    }

    [Fact]
    public void Create_WithEmptyFileUrl_ShouldFail()
    {
        // Act
        var result = Attachment.Create("file.pdf", "   ", 1024, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.InvalidFileUrl");
    }

    [Fact]
    public void Create_WithZeroFileSize_ShouldFail()
    {
        // Act
        var result = Attachment.Create("file.pdf", "https://storage/file", 0, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.InvalidFileSize");
    }

    [Fact]
    public void Create_WithNegativeFileSize_ShouldFail()
    {
        // Act
        var result = Attachment.Create("file.pdf", "https://storage/file", -100, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.InvalidFileSize");
    }

    [Fact]
    public void Create_WithFileSizeTooLarge_ShouldFail()
    {
        // Arrange
        var largeSize = 101L * 1024 * 1024; // 101 MB

        // Act
        var result = Attachment.Create("file.pdf", "https://storage/file", largeSize, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.FileTooLarge");
    }

    [Fact]
    public void Create_WithMaxAllowedFileSize_ShouldSucceed()
    {
        // Arrange
        var maxSize = 100L * 1024 * 1024; // Exactly 100 MB

        // Act
        var result = Attachment.Create("file.pdf", "https://storage/file", maxSize, "application/pdf");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyMimeType_ShouldFail()
    {
        // Act
        var result = Attachment.Create("file.pdf", "https://storage/file", 1024, "   ");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Attachment.InvalidMimeType");
    }

    [Fact]
    public void Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var attachment1 = Attachment.Create("file.pdf", "https://storage/file", 1024, "application/pdf").Value;
        var attachment2 = Attachment.Create("file.pdf", "https://storage/file", 1024, "application/pdf").Value;

        // Assert
        attachment1.Should().Be(attachment2);
        (attachment1 == attachment2).Should().BeTrue();
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var attachment1 = Attachment.Create("file1.pdf", "https://storage/file1", 1024, "application/pdf").Value;
        var attachment2 = Attachment.Create("file2.pdf", "https://storage/file2", 2048, "application/pdf").Value;

        // Assert
        attachment1.Should().NotBe(attachment2);
        (attachment1 == attachment2).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_WithSameValues_ShouldReturnSameHash()
    {
        // Arrange
        var attachment1 = Attachment.Create("file.pdf", "https://storage/file", 1024, "application/pdf").Value;
        var attachment2 = Attachment.Create("file.pdf", "https://storage/file", 1024, "application/pdf").Value;

        // Assert
        attachment1.GetHashCode().Should().Be(attachment2.GetHashCode());
    }
}
