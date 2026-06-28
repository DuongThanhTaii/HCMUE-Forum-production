using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Queries.Documents.GetDocumentById;

/// <summary>
/// Query to get a document by its ID.
/// </summary>
public sealed record GetDocumentByIdQuery(Guid DocumentId) : IRequest<Result<DocumentDetailResponse>>;

/// <summary>
/// Detailed response for a single document.
/// </summary>
public sealed record DocumentDetailResponse(
    Guid Id,
    string Title,
    string Description,
    string DocumentType,
    string Status,
    string FileName,
    string FilePath,
    long FileSize,
    string ContentType,
    double AverageRating,
    int RatingCount,
    int ViewCount,
    int DownloadCount,
    Guid UploaderId,
    Guid? CourseId,
    Guid? ReviewerId,
    string? ReviewComment,
    string? RejectionReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? SubmittedAt,
    DateTime? ReviewedAt,
    string? UploaderDisplayName,
    string? CourseName,
    string? ReviewerDisplayName);
