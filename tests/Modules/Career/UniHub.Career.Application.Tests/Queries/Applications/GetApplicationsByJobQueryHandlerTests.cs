using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.Applications.GetApplicationsByJob;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using Xunit;
using DomainApplication = UniHub.Career.Domain.Applications.Application;

namespace UniHub.Career.Application.Tests.Queries.Applications;

public class GetApplicationsByJobQueryHandlerTests
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly GetApplicationsByJobQueryHandler _handler;

    public GetApplicationsByJobQueryHandlerTests()
    {
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _handler = new GetApplicationsByJobQueryHandler(_applicationRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedApplications()
    {
        // Arrange
        var jobPostingId = Guid.NewGuid();

        var applications = new List<DomainApplication>
        {
            DomainApplication.Submit(
                JobPostingId.Create(jobPostingId),
                Guid.NewGuid(),
                Resume.Create("resume1.pdf", "https://example.com/resume1.pdf", 1024000, "application/pdf").Value).Value,
            DomainApplication.Submit(
                JobPostingId.Create(jobPostingId),
                Guid.NewGuid(),
                Resume.Create("resume2.pdf", "https://example.com/resume2.pdf", 1024000, "application/pdf").Value).Value,
            DomainApplication.Submit(
                JobPostingId.Create(jobPostingId),
                Guid.NewGuid(),
                Resume.Create("resume3.pdf", "https://example.com/resume3.pdf", 1024000, "application/pdf").Value).Value
        };

        _applicationRepository.GetByJobPostingAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<ApplicationStatus?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((applications, 3));

        var query = new GetApplicationsByJobQuery(jobPostingId, null, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(20);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ShouldFilterByStatus()
    {
        // Arrange
        var jobPostingId = Guid.NewGuid();
        var reviewerId = Guid.NewGuid();

        var application1 = DomainApplication.Submit(
            JobPostingId.Create(jobPostingId),
            Guid.NewGuid(),
            Resume.Create("resume1.pdf", "https://example.com/resume1.pdf", 1024000, "application/pdf").Value).Value;

        var application2 = DomainApplication.Submit(
            JobPostingId.Create(jobPostingId),
            Guid.NewGuid(),
            Resume.Create("resume2.pdf", "https://example.com/resume2.pdf", 1024000, "application/pdf").Value).Value;

        application2.MoveToReviewing(reviewerId);

        var applications = new List<DomainApplication> { application2 };

        _applicationRepository.GetByJobPostingAsync(
            Arg.Any<JobPostingId>(),
            ApplicationStatus.Reviewing,
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((applications, 1));

        var query = new GetApplicationsByJobQuery(jobPostingId, "Reviewing", 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items.First().Status.Should().Be("Reviewing");
    }

    [Fact]
    public async Task Handle_WithInvalidStatus_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetApplicationsByJobQuery(Guid.NewGuid(), "InvalidStatus", 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Application.InvalidStatus");
    }

    [Fact]
    public async Task Handle_WhenNoApplications_ShouldReturnEmptyList()
    {
        // Arrange
        _applicationRepository.GetByJobPostingAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<ApplicationStatus?>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<DomainApplication>(), 0));

        var query = new GetApplicationsByJobQuery(Guid.NewGuid(), null, 1, 20);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }
}
