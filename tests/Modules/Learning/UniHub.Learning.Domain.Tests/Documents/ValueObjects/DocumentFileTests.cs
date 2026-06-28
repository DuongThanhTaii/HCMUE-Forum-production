using UniHub.Learning.Domain.Documents.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Documents.ValueObjects;

public class DocumentFileTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var fileName = "document.pdf";
        var filePath = "/uploads/documents/document.pdf";
        var fileSize = 1024 * 1024; // 1 MB
        var contentType = "application/pdf";

        // Act
        var result = DocumentFile.Create(fileName, filePath, fileSize, contentType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be(fileName);
        result.Value.FilePath.Should().Be(filePath);
        result.Value.FileSize.Should().Be(fileSize);
        result.Value.ContentType.Should().Be(contentType);
        result.Value.FileExtension.Should().Be(".pdf");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyFileName_ShouldReturnFailure(string? invalidFileName)
    {
        // Act
        var result = DocumentFile.Create(invalidFileName, "/path", 1000, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentFile.EmptyName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyFilePath_ShouldReturnFailure(string? invalidFilePath)
    {
        // Act
        var result = DocumentFile.Create("file.pdf", invalidFilePath, 1000, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentFile.EmptyPath");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrEmptyContentType_ShouldReturnFailure(string? invalidContentType)
    {
        // Act
        var result = DocumentFile.Create("file.pdf", "/path", 1000, invalidContentType);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentFile.EmptyContentType");
    }

    [Theory]
    [InlineData("file.exe")]
    [InlineData("file.bat")]
    [InlineData("file.sh")]
    [InlineData("file")]
    public void Create_WithInvalidExtension_ShouldReturnFailure(string invalidFileName)
    {
        // Act
        var result = DocumentFile.Create(invalidFileName, "/path", 1000, "application/octet-stream");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentFile.InvalidExtension");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithInvalidFileSize_ShouldReturnFailure(long invalidSize)
    {
        // Act
        var result = DocumentFile.Create("file.pdf", "/path", invalidSize, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentFile.InvalidSize");
    }

    [Fact]
    public void Create_WithTooLargeFile_ShouldReturnFailure()
    {
        // Arrange
        var tooLargeSize = DocumentFile.MaxFileSize + 1;

        // Act
        var result = DocumentFile.Create("file.pdf", "/path", tooLargeSize, "application/pdf");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentFile.TooLarge");
    }

    [Fact]
    public void Create_WithMaxFileSize_ShouldReturnSuccess()
    {
        // Act
        var result = DocumentFile.Create("file.pdf", "/path", DocumentFile.MaxFileSize, "application/pdf");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".doc")]
    [InlineData(".docx")]
    [InlineData(".ppt")]
    [InlineData(".pptx")]
    [InlineData(".xls")]
    [InlineData(".xlsx")]
    [InlineData(".zip")]
    [InlineData(".rar")]
    [InlineData(".7z")]
    public void Create_WithAllowedExtensions_ShouldReturnSuccess(string extension)
    {
        // Arrange
        var fileName = $"file{extension}";

        // Act
        var result = DocumentFile.Create(fileName, "/path", 1000, "application/octet-stream");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("image.jpg", true)]
    [InlineData("image.jpeg", true)]
    [InlineData("image.png", true)]
    [InlineData("image.gif", true)]
    [InlineData("document.pdf", false)]
    [InlineData("video.mp4", false)]
    public void IsImage_ShouldReturnCorrectValue(string fileName, bool expected)
    {
        // Arrange
        var file = DocumentFile.Create(fileName, "/path", 1000, "application/octet-stream").Value;

        // Act
        var result = file.IsImage();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("video.mp4", true)]
    [InlineData("video.avi", true)]
    [InlineData("video.mov", true)]
    [InlineData("video.mkv", true)]
    [InlineData("document.pdf", false)]
    [InlineData("image.jpg", false)]
    public void IsVideo_ShouldReturnCorrectValue(string fileName, bool expected)
    {
        // Arrange
        var file = DocumentFile.Create(fileName, "/path", 1000, "application/octet-stream").Value;

        // Act
        var result = file.IsVideo();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("code.cs", true)]
    [InlineData("code.java", true)]
    [InlineData("code.py", true)]
    [InlineData("code.js", true)]
    [InlineData("readme.txt", true)]
    [InlineData("readme.md", true)]
    [InlineData("document.pdf", false)]
    public void IsCode_ShouldReturnCorrectValue(string fileName, bool expected)
    {
        // Arrange
        var file = DocumentFile.Create(fileName, "/path", 1000, "application/octet-stream").Value;

        // Act
        var result = file.IsCode();

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        // Arrange
        var file = DocumentFile.Create("test.pdf", "/path", 2048, "application/pdf").Value;

        // Act
        var result = file.ToString();

        // Assert
        result.Should().Be("test.pdf (2 KB)");
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var fileName = "  test.pdf  ";
        var filePath = "  /path/test.pdf  ";
        var contentType = "  application/pdf  ";

        // Act
        var result = DocumentFile.Create(fileName, filePath, 1000, contentType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("test.pdf");
        result.Value.FilePath.Should().Be("/path/test.pdf");
        result.Value.ContentType.Should().Be("application/pdf");
    }
}
