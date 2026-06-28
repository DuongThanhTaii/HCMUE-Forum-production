using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Documents.ValueObjects;

/// <summary>
/// File tài liệu (Value Object)
/// Chứa thông tin file: tên file, đường dẫn, kích thước, MIME type
/// </summary>
public sealed record DocumentFile
{
    public const long MaxFileSize = 50 * 1024 * 1024; // 50 MB
    
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".ppt", ".pptx",
        ".xls", ".xlsx", ".zip", ".rar", ".7z",
        ".jpg", ".jpeg", ".png", ".gif",
        ".mp4", ".avi", ".mov", ".mkv",
        ".txt", ".md", ".cs", ".java", ".py", ".js"
    };

    public string FileName { get; }
    public string FilePath { get; }
    public long FileSize { get; }
    public string ContentType { get; }
    public string? FileExtension { get; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private DocumentFile()
    {
        FileName = string.Empty;
        FilePath = string.Empty;
        ContentType = string.Empty;
    }

    private DocumentFile(string fileName, string filePath, long fileSize, string contentType)
    {
        FileName = fileName;
        FilePath = filePath;
        FileSize = fileSize;
        ContentType = contentType;
        FileExtension = Path.GetExtension(fileName);
    }

    public static Result<DocumentFile> Create(
        string? fileName,
        string? filePath,
        long fileSize,
        string? contentType)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure<DocumentFile>(
                new Error("DocumentFile.EmptyName", "File name cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result.Failure<DocumentFile>(
                new Error("DocumentFile.EmptyPath", "File path cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return Result.Failure<DocumentFile>(
                new Error("DocumentFile.EmptyContentType", "Content type cannot be empty"));
        }

        // Trim inputs before validation
        fileName = fileName.Trim();
        filePath = filePath.Trim();
        contentType = contentType.Trim();

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return Result.Failure<DocumentFile>(
                new Error("DocumentFile.InvalidExtension", 
                    $"File extension '{extension}' is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}"));
        }

        if (fileSize <= 0)
        {
            return Result.Failure<DocumentFile>(
                new Error("DocumentFile.InvalidSize", "File size must be greater than 0"));
        }

        if (fileSize > MaxFileSize)
        {
            return Result.Failure<DocumentFile>(
                new Error("DocumentFile.TooLarge", 
                    $"File size ({fileSize / 1024 / 1024} MB) exceeds maximum allowed ({MaxFileSize / 1024 / 1024} MB)"));
        }

        return Result.Success(new DocumentFile(fileName, filePath, fileSize, contentType));
    }

    public bool IsImage() => FileExtension switch
    {
        ".jpg" or ".jpeg" or ".png" or ".gif" => true,
        _ => false
    };

    public bool IsVideo() => FileExtension switch
    {
        ".mp4" or ".avi" or ".mov" or ".mkv" => true,
        _ => false
    };

    public bool IsCode() => FileExtension switch
    {
        ".cs" or ".java" or ".py" or ".js" or ".txt" or ".md" => true,
        _ => false
    };

    public override string ToString() => $"{FileName} ({FileSize / 1024} KB)";
}
