using UniHub.Forum.Domain.Reports;
using UniHub.SharedKernel.Domain;

namespace UniHub.Forum.Domain.Events;

/// <summary>
/// Raised when a user submits a moderation report (post or comment).
/// </summary>
public sealed record ReportSubmittedEvent(
    int ReportId,
    Guid ReportedItemId,
    ReportedItemType ReportedItemType,
    Guid ReporterId,
    ReportReason Reason) : IDomainEvent;
