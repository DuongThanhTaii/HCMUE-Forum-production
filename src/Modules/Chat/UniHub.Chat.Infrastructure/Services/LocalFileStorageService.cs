using UniHub.Chat.Application.Abstractions;
using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Infrastructure.Services;

/// <summary>
/// Local file storage implementation for chat files.
/// Stores files in wwwroot/uploads/chat directory.
/// Public URLs are stored as root-relative paths (/uploads/chat/...) so clients join their configured API origin (same host/port as REST).
/// TODO: Replace with cloud storage (Azure Blob, AWS S3, MinIO, etc.) for production.
/// </summary>
public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadPath;

    public LocalFileStorageService(string uploadPath)
    {
        _uploadPath = uploadPath;

        if (!Directory.Exists(_uploadPath))
        {
            Directory.CreateDirectory(_uploadPath);
        }
    }

    public async Task<Result<string>> UploadFileAsync(
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, uniqueFileName);

            using (var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);
            }

            // Root-relative path; Kestrel + UseStaticFiles serves from wwwroot
            var fileUrl = $"/uploads/chat/{uniqueFileName}";

            return Result.Success(fileUrl);
        }
        catch (Exception ex)
        {
            return Result.Failure<string>(new Error(
                "FileStorage.UploadFailed",
                $"Failed to upload file: {ex.Message}"));
        }
    }

    public Task<Result> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var name = GetStoredFileName(fileUrl);
            if (string.IsNullOrEmpty(name))
            {
                return Task.FromResult(Result.Failure(new Error("FileStorage.InvalidUrl", "Invalid file URL")));
            }

            var filePath = Path.Combine(_uploadPath, name);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.FromResult(Result.Success());
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure(new Error(
                "FileStorage.DeleteFailed",
                $"Failed to delete file: {ex.Message}")));
        }
    }

    public Task<Result<(Stream Stream, string ContentType)>> GetFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var name = GetStoredFileName(fileUrl);
            if (string.IsNullOrEmpty(name))
            {
                return Task.FromResult(Result.Failure<(Stream, string)>(new Error(
                    "FileStorage.InvalidUrl",
                    "Invalid file URL")));
            }

            var filePath = Path.Combine(_uploadPath, name);

            if (!File.Exists(filePath))
            {
                return Task.FromResult(Result.Failure<(Stream, string)>(new Error(
                    "FileStorage.FileNotFound",
                    "File not found")));
            }

            var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var contentType = GetContentType(Path.GetExtension(name));

            return Task.FromResult(Result.Success((stream as Stream, contentType)));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure<(Stream, string)>(new Error(
                "FileStorage.GetFileFailed",
                $"Failed to get file: {ex.Message}")));
        }
    }

    public Task<bool> FileExistsAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var name = GetStoredFileName(fileUrl);
            if (string.IsNullOrEmpty(name))
            {
                return Task.FromResult(false);
            }

            var filePath = Path.Combine(_uploadPath, name);
            return Task.FromResult(File.Exists(filePath));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    private static string GetStoredFileName(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return string.Empty;
        }

        var trimmed = fileUrl.Trim();

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var abs))
        {
            return Path.GetFileName(abs.LocalPath);
        }

        return Path.GetFileName(trimmed.Replace('\\', '/').TrimEnd('/'));
    }

    private string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".m4a" or ".mp3" or ".ogg" or ".wav" => "audio/mpeg",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
