using FluentAssertions;
using UniHub.Career.Domain.Applications;

namespace UniHub.Career.Domain.Tests.Applications;

public class ResumeTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = Resume.Create(
            "resume.pdf",
            "https://storage.example.com/resumes/123.pdf",
            1024 * 500, // 500 KB
            "application/pdf");

        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("resume.pdf");
        result.Value.FileUrl.Should().Be("https://storage.example.com/resumes/123.pdf");
        result.Value.FileSizeBytes.Should().Be(1024 * 500);
        result.Value.ContentType.Should().Be("application/pdf");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyFileName_ShouldReturnFailure(string? fileName)
    {
        var result = Resume.Create(
            fileName!,
            "https://example.com/resume.pdf",
            1024,
            "application/pdf");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Resume.FileNameEmpty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyFileUrl_ShouldReturnFailure(string? fileUrl)
    {
        var result = Resume.Create(
            "resume.pdf",
            fileUrl!,
            1024,
            "application/pdf");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Resume.FileUrlEmpty");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyContentType_ShouldReturnFailure(string? contentType)
    {
        var result = Resume.Create(
            "resume.pdf",
            "https://example.com/resume.pdf",
            1024,
            contentType!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Resume.ContentTypeEmpty");
    }

    [Fact]
    public void Create_WithFileNameTooLong_ShouldReturnFailure()
    {
        var longName = new string('a', Resume.MaxFileNameLength + 1) + ".pdf";

        var result = Resume.Create(
            longName,
            "https://example.com/resume.pdf",
            1024,
            "application/pdf");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Resume.FileNameTooLong");
    }

    [Fact]
    public void Create_WithFileUrlTooLong_ShouldReturnFailure()
    {
        var longUrl = "https://" + new string('a', Resume.MaxFileUrlLength);

        var result = Resume.Create(
            "resume.pdf",
            longUrl,
            1024,
            "application/pdf");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Resume.FileUrlTooLong");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Create_WithInvalidFileSize_ShouldReturnFailure(long fileSize)
    {
        var result = Resume.Create(
            "resume.pdf",
            "https://example.com/resume.pdf",
            fileSize,
            "application/pdf");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Resume.InvalidFileSize");
    }

    [Fact]
    public void Create_WithFileTooLarge_ShouldReturnFailure()
    {
        var result = Resume.Create(
            "resume.pdf",
            "https://example.com/resume.pdf",
            Resume.MaxFileSizeBytes + 1,
            "application/pdf");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Resume.FileTooLarge");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("application/msword")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    public void Create_WithAllowedContentType_ShouldSucceed(string contentType)
    {
        var result = Resume.Create(
            "resume.pdf",
            "https://example.com/resume.pdf",
            1024,
            contentType);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("image/jpeg")]
    [InlineData("text/plain")]
    [InlineData("application/zip")]
    public void Create_WithInvalidContentType_ShouldReturnFailure(string contentType)
    {
        var result = Resume.Create(
            "resume.jpg",
            "https://example.com/resume.jpg",
            1024,
            contentType);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Resume.InvalidContentType");
    }

    [Fact]
    public void Create_NormalizesContentTypeToLowerCase()
    {
        var result = Resume.Create(
            "resume.pdf",
            "https://example.com/resume.pdf",
            1024,
            "APPLICATION/PDF");

        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = Resume.Create(
            "  resume.pdf  ",
            "  https://example.com/resume.pdf  ",
            1024,
            "  application/pdf  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.FileName.Should().Be("resume.pdf");
        result.Value.FileUrl.Should().Be("https://example.com/resume.pdf");
        result.Value.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public void Equality_WithSameData_ShouldBeEqual()
    {
        var resume1 = Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024, "application/pdf").Value;
        var resume2 = Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024, "application/pdf").Value;

        resume1.Should().Be(resume2);
    }

    [Fact]
    public void Equality_WithDifferentData_ShouldNotBeEqual()
    {
        var resume1 = Resume.Create("resume1.pdf", "https://example.com/resume1.pdf", 1024, "application/pdf").Value;
        var resume2 = Resume.Create("resume2.pdf", "https://example.com/resume2.pdf", 2048, "application/pdf").Value;

        resume1.Should().NotBe(resume2);
    }
}
