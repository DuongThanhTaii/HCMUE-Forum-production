namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Interface for file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Upload file to storage and return the file path
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <param name="fileContent">File content as byte array</param>
    /// <param name="contentType">MIME type of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Relative path to the stored file</returns>
    Task<string> UploadFileAsync(string fileName, byte[] fileContent, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete file from storage
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get file content from storage
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File content as byte array</returns>
    Task<byte[]> GetFileAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if file exists in storage
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if file exists, false otherwise</returns>
    Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default);
}
