using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi document được tạo
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="UploaderId">ID người upload</param>
/// <param name="Type">Loại document</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentCreatedEvent(
    Guid DocumentId,
    Guid UploaderId,
    string Type,
    DateTime OccurredOn) : IDomainEvent;
