using UniHub.Learning.Domain.Documents.Events;
using UniHub.Learning.Domain.Documents.ValueObjects;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Documents;

/// <summary>
/// Document Aggregate Root - Tài liệu học tập
/// Implements Event Sourcing cho approval workflow history
/// </summary>
public sealed class Document : AggregateRoot<DocumentId>
{
    public DocumentTitle Title { get; private set; }
    public DocumentDescription Description { get; private set; }
    public DocumentFile File { get; private set; }
    public DocumentType Type { get; private set; }
    public DocumentStatus Status { get; private set; }
    
    /// <summary>
    /// ID của user upload tài liệu
    /// </summary>
    public Guid UploaderId { get; private set; }
    
    /// <summary>
    /// ID của course mà document này thuộc về (nullable - có thể thuộc nhiều course)
    /// </summary>
    public Guid? CourseId { get; private set; }
    
    /// <summary>
    /// ID của moderator approve/reject document
    /// </summary>
    public Guid? ReviewerId { get; private set; }
    
    /// <summary>
    /// Ghi chú từ moderator khi approve
    /// </summary>
    public string? ReviewComment { get; private set; }
    
    /// <summary>
    /// Lý do reject (bắt buộc khi Status = Rejected)
    /// </summary>
    public string? RejectionReason { get; private set; }
    
    /// <summary>
    /// Lượt download
    /// </summary>
    public int DownloadCount { get; private set; }
    
    /// <summary>
    /// Lượt xem
    /// </summary>
    public int ViewCount { get; private set; }
    
    /// <summary>
    /// Rating trung bình (1-5 stars)
    /// </summary>
    public double AverageRating { get; private set; }
    
    /// <summary>
    /// Tổng số lượng rating
    /// </summary>
    public int RatingCount { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    // EF Core constructor
    private Document()
    {
        Title = null!;
        Description = null!;
        File = null!;
    }

    private Document(
        DocumentId id,
        DocumentTitle title,
        DocumentDescription description,
        DocumentFile file,
        DocumentType type,
        Guid uploaderId,
        Guid? courseId)
    {
        Id = id;
        Title = title;
        Description = description;
        File = file;
        Type = type;
        Status = DocumentStatus.Draft;
        UploaderId = uploaderId;
        CourseId = courseId;
        DownloadCount = 0;
        ViewCount = 0;
        AverageRating = 0;
        RatingCount = 0;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method để tạo Document mới
    /// </summary>
    public static Result<Document> Create(
        DocumentTitle title,
        DocumentDescription description,
        DocumentFile file,
        DocumentType type,
        Guid uploaderId,
        Guid? courseId = null)
    {
        if (uploaderId == Guid.Empty)
        {
            return Result.Failure<Document>(
                new Error("Document.InvalidUploader", "Uploader ID cannot be empty"));
        }

        var document = new Document(
            DocumentId.CreateUnique(),
            title,
            description,
            file,
            type,
            uploaderId,
            courseId);

        // Event Sourcing - ghi lại sự kiện tạo document
        document.AddDomainEvent(new DocumentCreatedEvent(
            document.Id.Value,
            uploaderId,
            type.ToString(),
            DateTime.UtcNow));

        return Result.Success(document);
    }

    /// <summary>
    /// Update thông tin document (chỉ được update khi status = Draft hoặc Rejected)
    /// </summary>
    public Result Update(
        DocumentTitle title,
        DocumentDescription description,
        DocumentFile file,
        Guid? courseId)
    {
        if (Status == DocumentStatus.Deleted)
        {
            return Result.Failure(new Error("Document.Deleted", "Cannot update a deleted document"));
        }

        if (Status == DocumentStatus.Approved)
        {
            return Result.Failure(new Error("Document.Approved", 
                "Cannot update an approved document. Please submit a new version."));
        }

        if (Status == DocumentStatus.PendingApproval)
        {
            return Result.Failure(new Error("Document.PendingApproval", 
                "Cannot update a document that is pending approval. Please reject it first."));
        }

        Title = title;
        Description = description;
        File = file;
        CourseId = courseId;
        UpdatedAt = DateTime.UtcNow;

        // Event Sourcing - ghi lại sự kiện update
        AddDomainEvent(new DocumentUpdatedEvent(
            Id.Value,
            UploaderId,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Submit document để chờ phê duyệt
    /// </summary>
    public Result SubmitForApproval()
    {
        if (Status == DocumentStatus.Deleted)
        {
            return Result.Failure(new Error("Document.Deleted", "Cannot submit a deleted document"));
        }

        if (Status == DocumentStatus.PendingApproval)
        {
            return Result.Failure(new Error("Document.AlreadyPending", 
                "Document is already pending approval"));
        }

        if (Status == DocumentStatus.Approved)
        {
            return Result.Failure(new Error("Document.AlreadyApproved", 
                "Document is already approved"));
        }

        Status = DocumentStatus.PendingApproval;
        SubmittedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Event Sourcing - ghi lại sự kiện submit
        AddDomainEvent(new DocumentSubmittedForApprovalEvent(
            Id.Value,
            UploaderId,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Approve document (chỉ moderator mới được approve)
    /// </summary>
    public Result Approve(Guid reviewerId, string? comment = null)
    {
        if (reviewerId == Guid.Empty)
        {
            return Result.Failure(new Error("Document.InvalidReviewer", "Reviewer ID cannot be empty"));
        }

        if (Status == DocumentStatus.Deleted)
        {
            return Result.Failure(new Error("Document.Deleted", "Cannot approve a deleted document"));
        }

        if (Status != DocumentStatus.PendingApproval)
        {
            return Result.Failure(new Error("Document.NotPending", 
                "Only pending documents can be approved"));
        }

        Status = DocumentStatus.Approved;
        ReviewerId = reviewerId;
        ReviewComment = comment;
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        RejectionReason = null; // Clear rejection reason if any

        // Event Sourcing - ghi lại sự kiện approve
        AddDomainEvent(new DocumentApprovedEvent(
            Id.Value,
            reviewerId,
            comment,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Reject document (bắt buộc phải có lý do)
    /// </summary>
    public Result Reject(Guid reviewerId, string reason)
    {
        if (reviewerId == Guid.Empty)
        {
            return Result.Failure(new Error("Document.InvalidReviewer", "Reviewer ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(new Error("Document.MissingRejectionReason", 
                "Rejection reason is required"));
        }

        if (reason.Length < 10)
        {
            return Result.Failure(new Error("Document.RejectionReasonTooShort", 
                "Rejection reason must be at least 10 characters"));
        }

        if (Status == DocumentStatus.Deleted)
        {
            return Result.Failure(new Error("Document.Deleted", "Cannot reject a deleted document"));
        }

        if (Status != DocumentStatus.PendingApproval)
        {
            return Result.Failure(new Error("Document.NotPending", 
                "Only pending documents can be rejected"));
        }

        Status = DocumentStatus.Rejected;
        ReviewerId = reviewerId;
        RejectionReason = reason.Trim();
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Event Sourcing - ghi lại sự kiện reject
        AddDomainEvent(new DocumentRejectedEvent(
            Id.Value,
            reviewerId,
            reason.Trim(),
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// AI scan document để kiểm tra nội dung (automated check sau khi submit)
    /// </summary>
    public Result RecordAIScan(string scanResult, double confidence, List<string>? flaggedReasons = null)
    {
        if (string.IsNullOrWhiteSpace(scanResult))
        {
            return Result.Failure(new Error("Document.InvalidScanResult", "Scan result cannot be empty"));
        }

        if (confidence < 0 || confidence > 1)
        {
            return Result.Failure(new Error("Document.InvalidConfidence", 
                "Confidence must be between 0 and 1"));
        }

        if (Status != DocumentStatus.PendingApproval)
        {
            return Result.Failure(new Error("Document.NotPending", 
                "Only pending documents can be scanned"));
        }

        // Event Sourcing - ghi lại sự kiện AI scan
        AddDomainEvent(new DocumentAIScannedEvent(
            Id.Value,
            scanResult,
            confidence,
            flaggedReasons,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Moderator bắt đầu review document
    /// </summary>
    public Result StartReview(Guid reviewerId)
    {
        if (reviewerId == Guid.Empty)
        {
            return Result.Failure(new Error("Document.InvalidReviewer", "Reviewer ID cannot be empty"));
        }

        if (Status == DocumentStatus.Deleted)
        {
            return Result.Failure(new Error("Document.Deleted", "Cannot review a deleted document"));
        }

        if (Status != DocumentStatus.PendingApproval)
        {
            return Result.Failure(new Error("Document.NotPending", 
                "Only pending documents can be reviewed"));
        }

        ReviewerId = reviewerId;
        UpdatedAt = DateTime.UtcNow;

        // Event Sourcing - ghi lại sự kiện bắt đầu review
        AddDomainEvent(new DocumentReviewStartedEvent(
            Id.Value,
            reviewerId,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Yêu cầu chỉnh sửa document (moderator không approve cũng không reject, yêu cầu author sửa)
    /// </summary>
    public Result RequestRevision(Guid requestedBy, string reason, string? notes = null)
    {
        if (requestedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Document.InvalidRequester", "Requester ID cannot be empty"));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure(new Error("Document.MissingRevisionReason", 
                "Revision reason is required"));
        }

        if (reason.Length < 10)
        {
            return Result.Failure(new Error("Document.RevisionReasonTooShort", 
                "Revision reason must be at least 10 characters"));
        }

        if (Status == DocumentStatus.Deleted)
        {
            return Result.Failure(new Error("Document.Deleted", "Cannot request revision for a deleted document"));
        }

        if (Status != DocumentStatus.PendingApproval)
        {
            return Result.Failure(new Error("Document.NotPending", 
                "Only pending documents can have revision requested"));
        }

        // Khi request revision, document quay về trạng thái Draft để author sửa
        Status = DocumentStatus.Draft;
        ReviewerId = requestedBy;
        RejectionReason = reason.Trim();
        ReviewComment = notes?.Trim();
        ReviewedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        // Event Sourcing - ghi lại sự kiện request revision
        AddDomainEvent(new DocumentRevisionRequestedEvent(
            Id.Value,
            requestedBy,
            reason.Trim(),
            notes?.Trim(),
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Xóa document (soft delete)
    /// </summary>
    public Result Delete(Guid deleterId)
    {
        if (deleterId == Guid.Empty)
        {
            return Result.Failure(new Error("Document.InvalidDeleter", "Deleter ID cannot be empty"));
        }

        if (Status == DocumentStatus.Deleted)
        {
            return Result.Failure(new Error("Document.AlreadyDeleted", "Document is already deleted"));
        }

        Status = DocumentStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        // Event Sourcing - ghi lại sự kiện delete
        AddDomainEvent(new DocumentDeletedEvent(
            Id.Value,
            deleterId,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Tăng view count khi user xem document
    /// </summary>
    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Tăng download count khi user download document
    /// </summary>
    public void IncrementDownloadCount()
    {
        DownloadCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Thêm rating cho document (1-5 stars)
    /// </summary>
    public Result AddRating(int rating)
    {
        if (rating < 1 || rating > 5)
        {
            return Result.Failure(new Error("Document.InvalidRating", "Rating must be between 1 and 5"));
        }

        if (Status != DocumentStatus.Approved)
        {
            return Result.Failure(new Error("Document.NotApproved", 
                "Only approved documents can be rated"));
        }

        // Recalculate average rating
        var totalRating = (AverageRating * RatingCount) + rating;
        RatingCount++;
        AverageRating = totalRating / RatingCount;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Kiểm tra xem document có được approve hay chưa
    /// </summary>
    public bool IsApproved() => Status == DocumentStatus.Approved;

    /// <summary>
    /// Kiểm tra xem document có đang pending approval hay không
    /// </summary>
    public bool IsPending() => Status == DocumentStatus.PendingApproval;

    /// <summary>
    /// Kiểm tra xem document có bị reject hay không
    /// </summary>
    public bool IsRejected() => Status == DocumentStatus.Rejected;
}
