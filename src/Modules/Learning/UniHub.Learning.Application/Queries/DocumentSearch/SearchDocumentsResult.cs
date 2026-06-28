namespace UniHub.Learning.Application.Queries.DocumentSearch;

/// <summary>
/// Result of document search query with pagination.
/// </summary>
public sealed record SearchDocumentsResult(
    IReadOnlyList<DocumentSearchDto> Documents,
    int TotalCount,
    int PageNumber,
    int PageSize,
    int TotalPages);

/// <summary>
/// Document DTO for search results.
/// </summary>
public sealed record DocumentSearchDto(
    Guid Id,
    string Title,
    string Description,
    string DocumentType,
    string Status,
    string FileName,
    long FileSize,
    string ContentType,
    decimal AverageRating,
    int RatingCount,
    int ViewCount,
    int DownloadCount,
    Guid UploaderId,
    Guid? CourseId,
    DateTime CreatedAt,
    DateTime UpdatedAt);
