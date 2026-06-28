using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Queries.DocumentSearch;

/// <summary>
/// Query to search documents with filtering and sorting.
/// </summary>
public sealed record SearchDocumentsQuery(
    string? SearchTerm = null,
    Guid? CourseId = null,
    Guid? FacultyId = null,
    int? DocumentType = null,
    int? Status = null,
    DocumentSortBy SortBy = DocumentSortBy.CreatedDate,
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<SearchDocumentsResult>;

/// <summary>
/// Sorting options for document search.
/// </summary>
public enum DocumentSortBy
{
    CreatedDate,
    Title,
    Rating,
    Downloads,
    ViewCount
}
