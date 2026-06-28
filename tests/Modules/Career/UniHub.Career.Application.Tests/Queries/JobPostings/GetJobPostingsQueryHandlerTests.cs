using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.JobPostings.GetJobPostings;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using Xunit;

namespace UniHub.Career.Application.Tests.Queries.JobPostings;

public class GetJobPostingsQueryHandlerTests
{
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly GetJobPostingsQueryHandler _handler;

    public GetJobPostingsQueryHandlerTests()
    {
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _companyRepository = Substitute.For<ICompanyRepository>();
        _companyRepository
            .GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Company?>(null));
        _handler = new GetJobPostingsQueryHandler(_jobPostingRepository, _companyRepository);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ShouldReturnAllJobPostings()
    {
        // Arrange
        var jobPostings = new List<JobPosting>
        {
            CreateJobPosting("Backend Developer", JobType.FullTime, ExperienceLevel.Mid, "Ha Noi"),
            CreateJobPosting("Frontend Developer", JobType.Remote, ExperienceLevel.Junior, "Ho Chi Minh City"),
            CreateJobPosting("DevOps Engineer", JobType.FullTime, ExperienceLevel.Senior, "Da Nang")
        };

        _jobPostingRepository.GetAllAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<Guid?>(),
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((jobPostings, 3));

        var query = new GetJobPostingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.Page.Should().Be(1);
        result.Value.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var jobPostings = new List<JobPosting>
        {
            CreateJobPosting("Job 1", JobType.FullTime, ExperienceLevel.Mid, "Ha Noi"),
            CreateJobPosting("Job 2", JobType.FullTime, ExperienceLevel.Mid, "Ha Noi")
        };

        _jobPostingRepository.GetAllAsync(
            2,
            2,
            Arg.Any<Guid?>(),
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((jobPostings, 10));

        var query = new GetJobPostingsQuery(Page: 2, PageSize: 2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(10);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(2);
        result.Value.TotalPages.Should().Be(5);
    }

    [Fact]
    public async Task Handle_WithCompanyIdFilter_ShouldFilterByCompany()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var jobPostings = new List<JobPosting>
        {
            CreateJobPosting("Job A", JobType.FullTime, ExperienceLevel.Mid, "Ha Noi", companyId),
            CreateJobPosting("Job B", JobType.Remote, ExperienceLevel.Senior, "Ho Chi Minh City", companyId)
        };

        _jobPostingRepository.GetAllAsync(
            1,
            20,
            companyId,
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((jobPostings, 2));

        var query = new GetJobPostingsQuery(CompanyId: companyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.Items.Should().OnlyContain(j => j.CompanyId == companyId);
    }

    [Fact]
    public async Task Handle_WithMultipleFilters_ShouldCombineFilters()
    {
        // Arrange
        var jobPostings = new List<JobPosting>
        {
            CreateJobPosting("Remote Mid-Level Job", JobType.Remote, ExperienceLevel.Mid, "Ha Noi")
        };

        _jobPostingRepository.GetAllAsync(
            1,
            20,
            Arg.Any<Guid?>(),
            JobType.Remote,
            ExperienceLevel.Mid,
            JobPostingStatus.Published,
            "Ha Noi",
            true,
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((jobPostings, 1));

        var query = new GetJobPostingsQuery(
            JobType: JobType.Remote,
            ExperienceLevel: ExperienceLevel.Mid,
            Status: JobPostingStatus.Published,
            City: "Ha Noi",
            IsRemote: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithNoResults_ShouldReturnEmptyList()
    {
        // Arrange
        _jobPostingRepository.GetAllAsync(
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Any<Guid?>(),
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<JobPosting>(), 0));

        var query = new GetJobPostingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    private static JobPosting CreateJobPosting(
        string title,
        JobType jobType,
        ExperienceLevel experienceLevel,
        string city,
        Guid? companyId = null)
    {
        return JobPosting.Create(
            title,
            "Job description.",
            companyId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            jobType,
            experienceLevel,
            WorkLocation.Create(city).Value,
            null,
            null).Value;
    }
}
