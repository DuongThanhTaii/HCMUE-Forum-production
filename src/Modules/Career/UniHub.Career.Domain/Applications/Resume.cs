using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Applications;

/// <summary>
/// Value object representing resume/CV information.
/// </summary>
public sealed class Resume : ValueObject
{
    /// <summary>File name of the resume.</summary>
    public string FileName { get; private set; }

    /// <summary>URL or path to the stored resume file.</summary>
    public string FileUrl { get; private set; }

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>MIME type of the file.</summary>
    public string ContentType { get; private set; }

    public const int MaxFileNameLength = 255;
    public const int MaxFileUrlLength = 2000;
    public const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly string[] AllowedContentTypes = new[]
    {
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    /// <summary>Private constructor for EF Core.</summary>
    private Resume()
    {
        FileName = string.Empty;
        FileUrl = string.Empty;
        ContentType = string.Empty;
    }

    private Resume(string fileName, string fileUrl, long fileSizeBytes, string contentType)
    {
        FileName = fileName;
        FileUrl = fileUrl;
        FileSizeBytes = fileSizeBytes;
        ContentType = contentType;
    }

    /// <summary>
    /// Creates a new Resume value object.
    /// </summary>
    public static Result<Resume> Create(
        string fileName,
        string fileUrl,
        long fileSizeBytes,
        string contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<Resume>(
                new Error("Resume.FileNameEmpty", "Resume file name is required."));

        if (string.IsNullOrWhiteSpace(fileUrl))
            return Result.Failure<Resume>(
                new Error("Resume.FileUrlEmpty", "Resume file URL is required."));

        if (string.IsNullOrWhiteSpace(contentType))
            return Result.Failure<Resume>(
                new Error("Resume.ContentTypeEmpty", "Resume content type is required."));

        fileName = fileName.Trim();
        fileUrl = fileUrl.Trim();
        contentType = contentType.Trim().ToLowerInvariant();

        if (fileName.Length > MaxFileNameLength)
            return Result.Failure<Resume>(
                new Error("Resume.FileNameTooLong", $"File name cannot exceed {MaxFileNameLength} characters."));

        if (fileUrl.Length > MaxFileUrlLength)
            return Result.Failure<Resume>(
                new Error("Resume.FileUrlTooLong", $"File URL cannot exceed {MaxFileUrlLength} characters."));

        if (fileSizeBytes <= 0)
            return Result.Failure<Resume>(
                new Error("Resume.InvalidFileSize", "File size must be greater than 0."));

        if (fileSizeBytes > MaxFileSizeBytes)
            return Result.Failure<Resume>(
                new Error("Resume.FileTooLarge", $"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB."));

        if (!AllowedContentTypes.Contains(contentType))
            return Result.Failure<Resume>(
                new Error("Resume.InvalidContentType", "Only PDF, DOC, and DOCX files are allowed."));

        return Result.Success(new Resume(fileName, fileUrl, fileSizeBytes, contentType));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FileName;
        yield return FileUrl;
        yield return FileSizeBytes;
        yield return ContentType;
    }
}
