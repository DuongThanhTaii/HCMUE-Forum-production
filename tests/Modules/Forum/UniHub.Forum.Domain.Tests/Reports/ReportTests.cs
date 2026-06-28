using FluentAssertions;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Domain.Tests.Reports;

public class ReportTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var reportId = new ReportId(1);
        var reportedItemId = Guid.NewGuid();
        var reportedItemType = ReportedItemType.Post;
        var reporterId = Guid.NewGuid();
        var reason = ReportReason.Spam;
        var description = "This is spam content";

        // Act
        var result = Report.Create(
            reportId,
            reportedItemId,
            reportedItemType,
            reporterId,
            reason,
            description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ReportedItemId.Should().Be(reportedItemId);
        result.Value.ReportedItemType.Should().Be(reportedItemType);
        result.Value.ReporterId.Should().Be(reporterId);
        result.Value.Reason.Should().Be(reason);
        result.Value.Description.Should().Be(description);
        result.Value.Status.Should().Be(ReportStatus.Pending);
        result.Value.ReviewedAt.Should().BeNull();
        result.Value.ReviewedBy.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSucceed()
    {
        // Arrange
        var reportId = new ReportId(1);
        var reportedItemId = Guid.NewGuid();
        var reportedItemType = ReportedItemType.Comment;
        var reporterId = Guid.NewGuid();
        var reason = ReportReason.Harassment;

        // Act
        var result = Report.Create(
            reportId,
            reportedItemId,
            reportedItemType,
            reporterId,
            reason,
            null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public void Create_WithInvalidReportedItemId_ShouldFail()
    {
        // Arrange
        var reportId = new ReportId(1);
        var reportedItemId = Guid.Empty;
        var reportedItemType = ReportedItemType.Post;
        var reporterId = Guid.NewGuid();
        var reason = ReportReason.Spam;

        // Act
        var result = Report.Create(
            reportId,
            reportedItemId,
            reportedItemType,
            reporterId,
            reason,
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReportedItemId);
    }

    [Fact]
    public void Create_WithInvalidReporterId_ShouldFail()
    {
        // Arrange
        var reportId = new ReportId(1);
        var reportedItemId = Guid.NewGuid();
        var reportedItemType = ReportedItemType.Post;
        var reporterId = Guid.Empty;
        var reason = ReportReason.Spam;

        // Act
        var result = Report.Create(
            reportId,
            reportedItemId,
            reportedItemType,
            reporterId,
            reason,
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReporterId);
    }

    [Fact]
    public void Create_WithInvalidReason_ShouldFail()
    {
        // Arrange
        var reportId = new ReportId(1);
        var reportedItemId = Guid.NewGuid();
        var reportedItemType = ReportedItemType.Post;
        var reporterId = Guid.NewGuid();
        var reason = (ReportReason)999;

        // Act
        var result = Report.Create(
            reportId,
            reportedItemId,
            reportedItemType,
            reporterId,
            reason,
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReason);
    }

    [Fact]
    public void Create_WithInvalidReportedItemType_ShouldFail()
    {
        // Arrange
        var reportId = new ReportId(1);
        var reportedItemId = Guid.NewGuid();
        var reportedItemType = (ReportedItemType)999;
        var reporterId = Guid.NewGuid();
        var reason = ReportReason.Spam;

        // Act
        var result = Report.Create(
            reportId,
            reportedItemId,
            reportedItemType,
            reporterId,
            reason,
            null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReportedItemType);
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldFail()
    {
        // Arrange
        var reportId = new ReportId(1);
        var reportedItemId = Guid.NewGuid();
        var reportedItemType = ReportedItemType.Post;
        var reporterId = Guid.NewGuid();
        var reason = ReportReason.Spam;
        var description = new string('a', 1001);

        // Act
        var result = Report.Create(
            reportId,
            reportedItemId,
            reportedItemType,
            reporterId,
            reason,
            description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.DescriptionTooLong);
    }

    [Fact]
    public void MarkAsUnderReview_WithPendingStatus_ShouldSucceed()
    {
        // Arrange
        var report = CreateValidReport();
        var reviewerId = Guid.NewGuid();

        // Act
        var result = report.MarkAsUnderReview(reviewerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.UnderReview);
        report.ReviewedBy.Should().Be(reviewerId);
        report.ReviewedAt.Should().NotBeNull();
    }

    [Fact]
    public void MarkAsUnderReview_WithInvalidReviewerId_ShouldFail()
    {
        // Arrange
        var report = CreateValidReport();
        var reviewerId = Guid.Empty;

        // Act
        var result = report.MarkAsUnderReview(reviewerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReviewerId);
    }

    [Fact]
    public void MarkAsUnderReview_WithNonPendingStatus_ShouldFail()
    {
        // Arrange
        var report = CreateValidReport();
        report.MarkAsUnderReview(Guid.NewGuid());

        // Act
        var result = report.MarkAsUnderReview(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.CannotReviewNonPendingReport);
    }

    [Fact]
    public void Resolve_WithValidData_ShouldSucceed()
    {
        // Arrange
        var report = CreateValidReport();
        var reviewerId = Guid.NewGuid();

        // Act
        var result = report.Resolve(reviewerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Resolved);
        report.ReviewedBy.Should().Be(reviewerId);
        report.ReviewedAt.Should().NotBeNull();
    }

    [Fact]
    public void Resolve_WithInvalidReviewerId_ShouldFail()
    {
        // Arrange
        var report = CreateValidReport();
        var reviewerId = Guid.Empty;

        // Act
        var result = report.Resolve(reviewerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReviewerId);
    }

    [Fact]
    public void Resolve_WhenAlreadyResolved_ShouldFail()
    {
        // Arrange
        var report = CreateValidReport();
        report.Resolve(Guid.NewGuid());

        // Act
        var result = report.Resolve(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.AlreadyResolved);
    }

    [Fact]
    public void Resolve_WhenDismissed_ShouldFail()
    {
        // Arrange
        var report = CreateValidReport();
        report.Dismiss(Guid.NewGuid());

        // Act
        var result = report.Resolve(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.CannotResolveAfterDismiss);
    }

    [Fact]
    public void Dismiss_WithValidData_ShouldSucceed()
    {
        // Arrange
        var report = CreateValidReport();
        var reviewerId = Guid.NewGuid();

        // Act
        var result = report.Dismiss(reviewerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        report.Status.Should().Be(ReportStatus.Dismissed);
        report.ReviewedBy.Should().Be(reviewerId);
        report.ReviewedAt.Should().NotBeNull();
    }

    [Fact]
    public void Dismiss_WithInvalidReviewerId_ShouldFail()
    {
        // Arrange
        var report = CreateValidReport();
        var reviewerId = Guid.Empty;

        // Act
        var result = report.Dismiss(reviewerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReviewerId);
    }

    [Fact]
    public void Dismiss_WhenAlreadyDismissed_ShouldFail()
    {
        // Arrange
        var report = CreateValidReport();
        report.Dismiss(Guid.NewGuid());

        // Act
        var result = report.Dismiss(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.AlreadyDismissed);
    }

    [Fact]
    public void Dismiss_WhenResolved_ShouldFail()
    {
        // Arrange
        var report = CreateValidReport();
        report.Resolve(Guid.NewGuid());

        // Act
        var result = report.Dismiss(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.CannotDismissAfterResolve);
    }

    private static Report CreateValidReport()
    {
        var result = Report.Create(
            new ReportId(1),
            Guid.NewGuid(),
            ReportedItemType.Post,
            Guid.NewGuid(),
            ReportReason.Spam,
            "Test description");

        return result.Value;
    }
}
