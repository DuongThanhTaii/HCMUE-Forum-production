using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.Documents.Events;

/// <summary>
/// Domain event khi document được quét bởi AI để kiểm tra nội dung
/// </summary>
/// <param name="DocumentId">ID của document</param>
/// <param name="ScanResult">Kết quả quét (Pass, Flagged, Rejected)</param>
/// <param name="Confidence">Độ tin cậy của AI (0-1)</param>
/// <param name="FlaggedReasons">Các lý do bị cảnh báo (nếu có)</param>
/// <param name="OccurredOn">Thời điểm event xảy ra</param>
public sealed record DocumentAIScannedEvent(
    Guid DocumentId,
    string ScanResult,
    double Confidence,
    List<string>? FlaggedReasons,
    DateTime OccurredOn) : IDomainEvent;
