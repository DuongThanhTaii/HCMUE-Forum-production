using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Abstractions;

/// <summary>
/// Repository interface for managing reports.
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Gets a report by its ID.
    /// </summary>
    Task<Report?> GetByIdAsync(ReportId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has already reported a specific item.
    /// </summary>
    Task<Report?> GetByReporterAndItemAsync(
        Guid reporterId,
        Guid reportedItemId,
        ReportedItemType reportedItemType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated reports, optionally filtered by status.
    /// </summary>
    Task<(IReadOnlyList<Report> Reports, int TotalCount)> GetReportsAsync(
        int pageNumber,
        int pageSize,
        ReportStatus? status = null,
        ReportResolutionDecision? resolutionDecision = null,
        IReadOnlyList<Guid>? categoryIds = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new report.
    /// </summary>
    Task AddAsync(Report report, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing report.
    /// </summary>
    Task UpdateAsync(Report report, CancellationToken cancellationToken = default);
}
