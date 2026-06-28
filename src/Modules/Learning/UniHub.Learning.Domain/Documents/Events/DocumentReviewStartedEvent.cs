using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi moderator bắt đầu review document
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="ReviewerId">ID người review (moderator)</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentReviewStartedEvent(
    Guid DocumentId,
    Guid ReviewerId,
    DateTime OccurredOn) : IDomainEvent;
