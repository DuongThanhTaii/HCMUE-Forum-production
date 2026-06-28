using UniHub.SharedKernel.Results;

namespace UniHub.Chat.Application.Abstractions;

/// <summary>
/// Service for storing and retrieving chat files
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload a file and return its URL
    /// </summary>
    /// <param name="fileName">Original file name</param>
    /// <param name="fileStream">File content stream</param>
    /// <param name="contentType">MIME type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with file URL</returns>
    Task<Result<string>> UploadFileAsync(
        string fileName,
        Stream fileStream,
        string contentType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a file by its URL
    /// </summary>
    /// <param name="fileUrl">File URL to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<Result> DeleteFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file stream for download
    /// </summary>
    /// <param name="fileUrl">File URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with file stream and content type</returns>
    Task<Result<(Stream Stream, string ContentType)>> GetFileAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file exists
    /// </summary>
    /// <param name="fileUrl">File URL to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<bool> FileExistsAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);
}
