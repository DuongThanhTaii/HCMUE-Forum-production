using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi document được approve (phê duyệt)
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="ApproverId">ID người approve (moderator)</param>
/// <param name="ApprovalComment">Ghi chú khi approve</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentApprovedEvent(
    Guid DocumentId,
    Guid ApproverId,
    string? ApprovalComment,
    DateTime OccurredOn) : IDomainEvent;
