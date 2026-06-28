using UniHub.Learning.Application.Queries.DocumentSearch;
using UniHub.Learning.Domain.Documents;

namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Repository interface for Document aggregate
/// </summary>
public interface IDocumentRepository
{
    /// <summary>
    /// Add a new document
    /// </summary>
    Task AddAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing document
    /// </summary>
    Task UpdateAsync(Document document, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get document by ID
    /// </summary>
    Task<Document?> GetByIdAsync(DocumentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if document exists
    /// </summary>
    Task<bool> ExistsAsync(DocumentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete document
    /// </summary>
    Task DeleteAsync(DocumentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by course ID
    /// </summary>
    Task<IReadOnlyList<Document>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get documents by uploader ID
    /// </summary>
    Task<IReadOnlyList<Document>> GetByUploaderIdAsync(Guid uploaderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search documents with filtering, sorting, and pagination
    /// </summary>
    Task<(IReadOnlyList<Document> Documents, int TotalCount)> SearchAsync(
        string? searchTerm,
        Guid? courseId,
        Guid? facultyId,
        int? documentType,
        int? status,
        DocumentSortBy sortBy,
        bool sortDescending,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
