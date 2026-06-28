using Microsoft.AspNetCore.Http;
using UniHub.Learning.Application.Queries.DocumentSearch;

namespace UniHub.Learning.Presentation.DTOs.Documents;

public record SearchDocumentsRequest(
    string? SearchTerm = null,
    Guid? CourseId = null,
    Guid? FacultyId = null,
    int? DocumentType = null,
    int? Status = null,
    DocumentSortBy? SortBy = null,
    bool? SortDescending = null,
    int? PageNumber = null,
    int? PageSize = null);

public record UploadDocumentRequest(
    string Title,
    string? Description,
    IFormFile File,
    int DocumentType,
    Guid? UploaderId,
    Guid? CourseId);

public record UploadDocumentResponse(
    Guid DocumentId,
    string Title);

public record RateDocumentRequest(
    Guid UserId,
    int Rating);

public record DownloadDocumentRequest(
    Guid UserId);

public record ApproveDocumentRequest(
    Guid? ReviewerId,
    string? Comment);

public record RejectDocumentRequest(
    Guid? ReviewerId,
    string Reason);

public record RequestRevisionRequest(
    Guid? ReviewerId,
    string Reason);
