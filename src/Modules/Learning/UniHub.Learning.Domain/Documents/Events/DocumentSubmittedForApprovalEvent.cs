using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi document được submit để phê duyệt
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="SubmitterId">ID người submit</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentSubmittedForApprovalEvent(
    Guid DocumentId,
    Guid SubmitterId,
    DateTime OccurredOn) : IDomainEvent;
