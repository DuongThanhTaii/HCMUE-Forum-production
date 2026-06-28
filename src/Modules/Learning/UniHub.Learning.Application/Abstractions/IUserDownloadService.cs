namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Service to track user document downloads
/// </summary>
public interface IUserDownloadService
{
    /// <summary>
    /// Check if a user has already downloaded a document
    /// </summary>
    Task<bool> HasUserDownloadedDocumentAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a user download
    /// </summary>
    Task RecordUserDownloadAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken = default);
}
