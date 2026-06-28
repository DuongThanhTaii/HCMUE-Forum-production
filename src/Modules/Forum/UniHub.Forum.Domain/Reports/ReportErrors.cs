using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Reports;

public static class ReportErrors
{
    public static readonly Error InvalidReportedItemId = new(
        "Report.InvalidReportedItemId",
        "Reported item ID cannot be empty");

    public static readonly Error InvalidReporterId = new(
        "Report.InvalidReporterId",
        "Reporter ID cannot be empty");

    public static readonly Error InvalidReviewerId = new(
        "Report.InvalidReviewerId",
        "Reviewer ID cannot be empty");

    public static readonly Error InvalidResolutionDecision = new(
        "Report.InvalidResolutionDecision",
        "Resolution decision is required");

    public static readonly Error InvalidReason = new(
        "Report.InvalidReason",
        "Invalid report reason");

    public static readonly Error InvalidReportedItemType = new(
        "Report.InvalidReportedItemType",
        "Invalid reported item type");

    public static readonly Error DescriptionTooLong = new(
        "Report.DescriptionTooLong",
        "Description cannot exceed 1000 characters");

    public static readonly Error CannotReviewNonPendingReport = new(
        "Report.CannotReviewNonPendingReport",
        "Can only mark pending reports as under review");

    public static readonly Error AlreadyResolved = new(
        "Report.AlreadyResolved",
        "Report is already resolved");

    public static readonly Error AlreadyDismissed = new(
        "Report.AlreadyDismissed",
        "Report is already dismissed");

    public static readonly Error CannotResolveAfterDismiss = new(
        "Report.CannotResolveAfterDismiss",
        "Cannot resolve a dismissed report");

    public static readonly Error CannotDismissAfterResolve = new(
        "Report.CannotDismissAfterResolve",
        "Cannot dismiss a resolved report");

    public static readonly Error ResolutionDecisionRequired = new(
        "Report.ResolutionDecisionRequired",
        "Resolved reports must include a resolution decision");

    public static readonly Error ReportNotFound = new(
        "Report.NotFound",
        "Report not found");

    public static readonly Error DuplicateReport = new(
        "Report.DuplicateReport",
        "You have already reported this item");

    public static readonly Error PostNotFound = new(
        "Report.PostNotFound",
        "Post not found");

    public static readonly Error CommentNotFound = new(
        "Report.CommentNotFound",
        "Comment not found");

    public static readonly Error InvalidPageNumber = new(
        "Report.InvalidPageNumber",
        "Page number must be greater than 0");

    public static readonly Error InvalidPageSize = new(
        "Report.InvalidPageSize",
        "Page size must be between 1 and 100");

    public static readonly Error Forbidden = new(
        "Moderation.Forbidden",
        "You are not authorized to resolve this report. It belongs to a category outside your moderation scope.");
}
