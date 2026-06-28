using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi document được update
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="UpdaterId">ID người update</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentUpdatedEvent(
    Guid DocumentId,
    Guid UpdaterId,
    DateTime OccurredOn) : IDomainEvent;
