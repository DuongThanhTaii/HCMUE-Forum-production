using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Reports;

public sealed class Report : Entity<ReportId>
{
    /// <summary>Private parameterless constructor for EF Core.</summary>
    private Report() { }

    private Report(
        ReportId id,
        Guid reportedItemId,
        ReportedItemType reportedItemType,
        Guid reporterId,
        ReportReason reason,
        string? description)
        : base(id)
    {
        ReportedItemId = reportedItemId;
        ReportedItemType = reportedItemType;
        ReporterId = reporterId;
        Reason = reason;
        Description = description;
        Status = ReportStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid ReportedItemId { get; private set; }
    public ReportedItemType ReportedItemType { get; private set; }
    public Guid ReporterId { get; private set; }
    public ReportReason Reason { get; private set; }
    public string? Description { get; private set; }
    public ReportStatus Status { get; private set; }
    public ReportResolutionDecision? ResolutionDecision { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ReviewedBy { get; private set; }

    public static Result<Report> Create(
        ReportId id,
        Guid reportedItemId,
        ReportedItemType reportedItemType,
        Guid reporterId,
        ReportReason reason,
        string? description)
    {
        if (reportedItemId == Guid.Empty)
        {
            return Result.Failure<Report>(ReportErrors.InvalidReportedItemId);
        }

        if (reporterId == Guid.Empty)
        {
            return Result.Failure<Report>(ReportErrors.InvalidReporterId);
        }

        if (!Enum.IsDefined(typeof(ReportReason), reason))
        {
            return Result.Failure<Report>(ReportErrors.InvalidReason);
        }

        if (!Enum.IsDefined(typeof(ReportedItemType), reportedItemType))
        {
            return Result.Failure<Report>(ReportErrors.InvalidReportedItemType);
        }

        if (description?.Length > 1000)
        {
            return Result.Failure<Report>(ReportErrors.DescriptionTooLong);
        }

        var report = new Report(
            id,
            reportedItemId,
            reportedItemType,
            reporterId,
            reason,
            description);

        return Result.Success(report);
    }

    public Result MarkAsUnderReview(Guid reviewerId)
    {
        if (reviewerId == Guid.Empty)
        {
            return Result.Failure(ReportErrors.InvalidReviewerId);
        }

        if (Status != ReportStatus.Pending)
        {
            return Result.Failure(ReportErrors.CannotReviewNonPendingReport);
        }

        Status = ReportStatus.UnderReview;
        ResolutionDecision = null;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Resolve(Guid reviewerId, ReportResolutionDecision decision)
    {
        if (reviewerId == Guid.Empty)
        {
            return Result.Failure(ReportErrors.InvalidReviewerId);
        }

        if (!Enum.IsDefined(typeof(ReportResolutionDecision), decision))
        {
            return Result.Failure(ReportErrors.InvalidResolutionDecision);
        }

        if (Status == ReportStatus.Resolved)
        {
            return Result.Failure(ReportErrors.AlreadyResolved);
        }

        if (Status == ReportStatus.Dismissed)
        {
            return Result.Failure(ReportErrors.CannotResolveAfterDismiss);
        }

        Status = ReportStatus.Resolved;
        ResolutionDecision = decision;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;

        return Result.Success();
    }

    public Result Dismiss(Guid reviewerId)
    {
        if (reviewerId == Guid.Empty)
        {
            return Result.Failure(ReportErrors.InvalidReviewerId);
        }

        if (Status == ReportStatus.Dismissed)
        {
            return Result.Failure(ReportErrors.AlreadyDismissed);
        }

        if (Status == ReportStatus.Resolved)
        {
            return Result.Failure(ReportErrors.CannotDismissAfterResolve);
        }

        Status = ReportStatus.Dismissed;
        ResolutionDecision = null;
        ReviewedBy = reviewerId;
        ReviewedAt = DateTime.UtcNow;

        return Result.Success();
    }
}
