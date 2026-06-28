using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.SavedJobs.GetSavedJobs;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using Xunit;

namespace UniHub.Career.Application.Tests.Queries.SavedJobs;

public class GetSavedJobsQueryHandlerTests
{
    private readonly ISavedJobRepository _savedJobRepository;
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly GetSavedJobsQueryHandler _handler;

    public GetSavedJobsQueryHandlerTests()
    {
        _savedJobRepository = Substitute.For<ISavedJobRepository>();
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _companyRepository = Substitute.For<ICompanyRepository>();
        _handler = new GetSavedJobsQueryHandler(
            _savedJobRepository,
            _jobPostingRepository,
            _companyRepository);
    }

    [Fact]
    public async Task Handle_ShouldReturnPaginatedSavedJobs()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var jobPostingId1 = Guid.NewGuid();
        var jobPostingId2 = Guid.NewGuid();
        var companyId = Guid.NewGuid();

        var savedJobs = new List<SavedJob>
        {
            new() { UserId = userId, JobPostingId = jobPostingId1, SavedAt = DateTime.UtcNow.AddDays(-2) },
            new() { UserId = userId, JobPostingId = jobPostingId2, SavedAt = DateTime.UtcNow.AddDays(-1) }
        };

        var jobPosting1 = JobPosting.Create(
            "Software Engineer",
            "Description 1",
            companyId,
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            WorkLocation.Create("Hanoi", null, null, false).Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;

        var jobPosting2 = JobPosting.Create(
            "Frontend Developer",
            "Description 2",
            companyId,
            Guid.NewGuid(),
            JobType.PartTime,
            ExperienceLevel.Junior,
            WorkLocation.Create("Ho Chi Minh", null, null, true).Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;

        var company = Company.Register(
            "Tech Corp",
            "A tech company",
            Industry.Technology,
            CompanySize.Medium,
            ContactInfo.Create("contact@techcorp.com", "+84123456789", "123 Street").Value,
            Guid.NewGuid(),
            "https://techcorp.com",
            null,
            2020,
            null).Value;

        _savedJobRepository.GetSavedJobsByUserAsync(userId, 1, 10, Arg.Any<CancellationToken>())
            .Returns(savedJobs);

        _savedJobRepository.GetSavedCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(2);

        _jobPostingRepository.GetByIdAsync(JobPostingId.Create(jobPostingId1), Arg.Any<CancellationToken>())
            .Returns(jobPosting1);

        _jobPostingRepository.GetByIdAsync(JobPostingId.Create(jobPostingId2), Arg.Any<CancellationToken>())
            .Returns(jobPosting2);

        _companyRepository.GetByIdAsync(CompanyId.Create(companyId), Arg.Any<CancellationToken>())
            .Returns(company);

        var query = new GetSavedJobsQuery(userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalPages.Should().Be(1);

        var firstJob = result.Value.Items[0];
        firstJob.Title.Should().Be("Software Engineer");
        firstJob.CompanyName.Should().Be("Tech Corp");
        firstJob.JobType.Should().Be("FullTime");
        firstJob.Location.City.Should().Be("Hanoi");
    }

    [Fact]
    public async Task Handle_WhenNoSavedJobs_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _savedJobRepository.GetSavedJobsByUserAsync(userId, 1, 10, Arg.Any<CancellationToken>())
            .Returns(new List<SavedJob>());

        _savedJobRepository.GetSavedCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(0);

        var query = new GetSavedJobsQuery(userId, 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldCalculateTotalPages()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _savedJobRepository.GetSavedJobsByUserAsync(userId, 2, 5, Arg.Any<CancellationToken>())
            .Returns(new List<SavedJob>());

        _savedJobRepository.GetSavedCountAsync(userId, Arg.Any<CancellationToken>())
            .Returns(12);

        var query = new GetSavedJobsQuery(userId, 2, 5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPages.Should().Be(3); // 12 items / 5 per page = 3 pages
    }
}
