using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi document bị xóa
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="DeleterId">ID người xóa</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentDeletedEvent(
    Guid DocumentId,
    Guid DeleterId,
    DateTime OccurredOn) : IDomainEvent;
