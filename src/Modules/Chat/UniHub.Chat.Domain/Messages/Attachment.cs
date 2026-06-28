using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Domain.Messages;

/// <summary>
/// Value object cho file attachment trong message
/// </summary>
public sealed class Attachment : ValueObject
{
    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; private set; }
    
    /// <summary>
    /// File URL hoặc storage path
    /// </summary>
    public string FileUrl { get; private set; }
    
    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; private set; }
    
    /// <summary>
    /// MIME type (image/png, application/pdf, etc.)
    /// </summary>
    public string MimeType { get; private set; }
    
    /// <summary>
    /// Thumbnail URL cho images/videos (optional)
    /// </summary>
    public string? ThumbnailUrl { get; private set; }

    private Attachment()
    {
        FileName = null!;
        FileUrl = null!;
        MimeType = null!;
    } // EF Core

    private Attachment(string fileName, string fileUrl, long fileSizeBytes, string mimeType, string? thumbnailUrl)
    {
        FileName = fileName;
        FileUrl = fileUrl;
        FileSizeBytes = fileSizeBytes;
        MimeType = mimeType;
        ThumbnailUrl = thumbnailUrl;
    }

    public static Result<Attachment> Create(
        string fileName,
        string fileUrl,
        long fileSizeBytes,
        string mimeType,
        string? thumbnailUrl = null)
    {
        // Validate file name
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return Result.Failure<Attachment>(new Error("Attachment.InvalidFileName", "File name cannot be empty"));
        }

        if (fileName.Length > 255)
        {
            return Result.Failure<Attachment>(new Error("Attachment.FileNameTooLong", "File name cannot exceed 255 characters"));
        }

        // Validate file URL
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Result.Failure<Attachment>(new Error("Attachment.InvalidFileUrl", "File URL cannot be empty"));
        }

        // Validate file size (max 100 MB)
        if (fileSizeBytes <= 0)
        {
            return Result.Failure<Attachment>(new Error("Attachment.InvalidFileSize", "File size must be greater than 0"));
        }

        if (fileSizeBytes > 100 * 1024 * 1024) // 100 MB
        {
            return Result.Failure<Attachment>(new Error("Attachment.FileTooLarge", "File size cannot exceed 100 MB"));
        }

        // Validate MIME type
        if (string.IsNullOrWhiteSpace(mimeType))
        {
            return Result.Failure<Attachment>(new Error("Attachment.InvalidMimeType", "MIME type cannot be empty"));
        }

        return Result.Success(new Attachment(fileName, fileUrl, fileSizeBytes, mimeType, thumbnailUrl));
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FileName;
        yield return FileUrl;
        yield return FileSizeBytes;
        yield return MimeType;
        yield return ThumbnailUrl;
    }
}
