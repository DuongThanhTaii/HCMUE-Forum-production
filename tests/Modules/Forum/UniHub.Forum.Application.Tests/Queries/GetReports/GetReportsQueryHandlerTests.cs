using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Queries.GetReports;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Tests.Queries.GetReports;

public class GetReportsQueryHandlerTests
{
    private readonly IReportRepository _reportRepository;
    private readonly GetReportsQueryHandler _handler;

    public GetReportsQueryHandlerTests()
    {
        _reportRepository = Substitute.For<IReportRepository>();
        _handler = new GetReportsQueryHandler(_reportRepository);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnReports()
    {
        // Arrange
        var query = new GetReportsQuery(PageNumber: 1, PageSize: 20);
        var reports = CreateReportsList(5);

        _reportRepository.GetReportsAsync(
            query.PageNumber,
            query.PageSize,
            query.Status,
            Arg.Any<CancellationToken>())
            .Returns((reports, 5));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Reports.Should().HaveCount(5);
        result.Value.TotalCount.Should().Be(5);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalPages.Should().Be(1);
        result.Value.HasPreviousPage.Should().BeFalse();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithFilterByStatus_ShouldReturnFilteredReports()
    {
        // Arrange
        var query = new GetReportsQuery(
            PageNumber: 1,
            PageSize: 20,
            Status: ReportStatus.Pending);

        var reports = CreateReportsList(3, ReportStatus.Pending);

        _reportRepository.GetReportsAsync(
            query.PageNumber,
            query.PageSize,
            query.Status,
            Arg.Any<CancellationToken>())
            .Returns((reports, 3));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Reports.Should().HaveCount(3);
        result.Value.Reports.Should().AllSatisfy(r => r.Status.Should().Be(ReportStatus.Pending));
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var query = new GetReportsQuery(PageNumber: 2, PageSize: 10);
        var reports = CreateReportsList(10);

        _reportRepository.GetReportsAsync(
            query.PageNumber,
            query.PageSize,
            query.Status,
            Arg.Any<CancellationToken>())
            .Returns((reports, 25)); // Total 25 reports

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Reports.Should().HaveCount(10);
        result.Value.PageNumber.Should().Be(2);
        result.Value.TotalCount.Should().Be(25);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithLastPage_ShouldIndicateNoNextPage()
    {
        // Arrange
        var query = new GetReportsQuery(PageNumber: 3, PageSize: 10);
        var reports = CreateReportsList(5); // Only 5 reports on last page

        _reportRepository.GetReportsAsync(
            query.PageNumber,
            query.PageSize,
            query.Status,
            Arg.Any<CancellationToken>())
            .Returns((reports, 25)); // Total 25 reports

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Reports.Should().HaveCount(5);
        result.Value.PageNumber.Should().Be(3);
        result.Value.TotalPages.Should().Be(3);
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithInvalidPageNumber_ShouldFail()
    {
        // Arrange
        var query = new GetReportsQuery(PageNumber: 0, PageSize: 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidPageNumber);
    }

    [Fact]
    public async Task Handle_WithInvalidPageSize_ShouldFail()
    {
        // Arrange
        var query = new GetReportsQuery(PageNumber: 1, PageSize: 101);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidPageSize);
    }

    [Fact]
    public async Task Handle_WithPageSizeTooSmall_ShouldFail()
    {
        // Arrange
        var query = new GetReportsQuery(PageNumber: 1, PageSize: 0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidPageSize);
    }

    [Fact]
    public async Task Handle_WithNoReports_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetReportsQuery(PageNumber: 1, PageSize: 20);

        _reportRepository.GetReportsAsync(
            query.PageNumber,
            query.PageSize,
            query.Status,
            Arg.Any<CancellationToken>())
            .Returns((new List<Report>(), 0));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Reports.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
        result.Value.HasPreviousPage.Should().BeFalse();
        result.Value.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldMapReportPropertiesCorrectly()
    {
        // Arrange
        var query = new GetReportsQuery(PageNumber: 1, PageSize: 20);
        var reportId = 1;
        var reportedItemId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        var reviewedBy = Guid.NewGuid();
        var report = Report.Create(
            new ReportId(reportId),
            reportedItemId,
            ReportedItemType.Post,
            reporterId,
            ReportReason.Spam,
            "Test description").Value;

        report.MarkAsUnderReview(reviewedBy);

        _reportRepository.GetReportsAsync(
            query.PageNumber,
            query.PageSize,
            query.Status,
            Arg.Any<CancellationToken>())
            .Returns((new List<Report> { report }, 1));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var reportDto = result.Value.Reports.First();
        reportDto.Id.Should().Be(reportId);
        reportDto.ReportedItemId.Should().Be(reportedItemId);
        reportDto.ReportedItemType.Should().Be(ReportedItemType.Post);
        reportDto.ReporterId.Should().Be(reporterId);
        reportDto.Reason.Should().Be(ReportReason.Spam);
        reportDto.Description.Should().Be("Test description");
        reportDto.Status.Should().Be(ReportStatus.UnderReview);
        reportDto.ReviewedBy.Should().Be(reviewedBy);
        reportDto.ReviewedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithDifferentStatuses_ShouldFilterCorrectly()
    {
        // Arrange
        var statuses = new[]
        {
            ReportStatus.Pending,
            ReportStatus.UnderReview,
            ReportStatus.Resolved,
            ReportStatus.Dismissed
        };

        foreach (var status in statuses)
        {
            var query = new GetReportsQuery(PageNumber: 1, PageSize: 20, Status: status);
            var reports = CreateReportsList(5, status);

            _reportRepository.GetReportsAsync(
                query.PageNumber,
                query.PageSize,
                query.Status,
                Arg.Any<CancellationToken>())
                .Returns((reports, 5));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Reports.Should().AllSatisfy(r => r.Status.Should().Be(status));

            _reportRepository.ClearReceivedCalls();
        }
    }

    [Fact]
    public async Task Handle_WithDefaultParameters_ShouldUseDefaultValues()
    {
        // Arrange
        var query = new GetReportsQuery(); // PageNumber=1, PageSize=20, Status=null
        var reports = CreateReportsList(10);

        _reportRepository.GetReportsAsync(
            1,
            20,
            null,
            Arg.Any<CancellationToken>())
            .Returns((reports, 10));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
    }

    private static List<Report> CreateReportsList(int count, ReportStatus? status = null)
    {
        var reports = new List<Report>();

        for (int i = 0; i < count; i++)
        {
            var report = Report.Create(
                new ReportId(i + 1),
                Guid.NewGuid(),
                i % 2 == 0 ? ReportedItemType.Post : ReportedItemType.Comment,
                Guid.NewGuid(),
                ReportReason.Spam,
                $"Description {i + 1}").Value;

            if (status.HasValue)
            {
                switch (status.Value)
                {
                    case ReportStatus.UnderReview:
                        report.MarkAsUnderReview(Guid.NewGuid());
                        break;
                    case ReportStatus.Resolved:
                        report.Resolve(Guid.NewGuid());
                        break;
                    case ReportStatus.Dismissed:
                        report.Dismiss(Guid.NewGuid());
                        break;
                    case ReportStatus.Pending:
                    default:
                        // Already pending
                        break;
                }
            }

            reports.Add(report);
        }

        return reports;
    }
}
