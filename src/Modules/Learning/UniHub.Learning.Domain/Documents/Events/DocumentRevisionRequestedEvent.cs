using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi moderator yêu cầu chỉnh sửa document
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="RequestedBy">ID người yêu cầu revision (moderator)</param>
/// <param name="RevisionReason">Lý do yêu cầu chỉnh sửa</param>
/// <param name="RevisionNotes">Ghi chú chi tiết cho việc chỉnh sửa</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentRevisionRequestedEvent(
    Guid DocumentId,
    Guid RequestedBy,
    string RevisionReason,
    string? RevisionNotes,
    DateTime OccurredOn) : IDomainEvent;
