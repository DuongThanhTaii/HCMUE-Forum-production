using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi document bị reject (từ chối)
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="RejectorId">ID người reject (moderator)</param>
/// <param name="RejectionReason">Lý do từ chối (bắt buộc)</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentRejectedEvent(
    Guid DocumentId,
    Guid RejectorId,
    string RejectionReason,
    DateTime OccurredOn) : IDomainEvent;
