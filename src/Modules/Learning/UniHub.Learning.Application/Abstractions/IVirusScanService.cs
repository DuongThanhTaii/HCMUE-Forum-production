namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Interface for virus/malware scanning service
/// </summary>
public interface IVirusScanService
{
    /// <summary>
    /// Scan file content for viruses/malware
    /// </summary>
    /// <param name="fileContent">File content to scan</param>
    /// <param name="fileName">Name of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Scan result with status and details</returns>
    Task<VirusScanResult> ScanAsync(byte[] fileContent, string fileName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of virus scan
/// </summary>
public sealed record VirusScanResult(
    bool IsClean,
    string Status,
    string? Details = null);
