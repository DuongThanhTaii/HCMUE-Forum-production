using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.JobPostings.SearchJobPostings;
using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Application.Tests.Queries.JobPostings;

public class SearchJobPostingsQueryHandlerTests
{
    private readonly IJobPostingRepository _repository;
    private readonly SearchJobPostingsQueryHandler _handler;

    public SearchJobPostingsQueryHandlerTests()
    {
        _repository = Substitute.For<IJobPostingRepository>();
        _handler = new SearchJobPostingsQueryHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithKeywords_ShouldCalculateRelevanceScores()
    {
        // Arrange
        var location = WorkLocation.Create("Ho Chi Minh City", null, null, false).Value;
        
        var job1 = JobPosting.Create(
            "Senior Software Engineer",
            "Build scalable backend software systems",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Senior,
            location).Value;

        var job2 = JobPosting.Create(
            "Junior Developer",
            "Learn software engineering best practices",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Junior,
            location).Value;

        _repository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<decimal?>(),
            Arg.Any<decimal?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<JobPosting> { job1, job2 }, 2));

        var query = new SearchJobPostingsQuery(Keywords: "software engineer");

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        response.Items.Should().HaveCount(2);
        response.Items[0].RelevanceScore.Should().BeGreaterThan(0);
        
        // Job1 should have higher relevance (exact match in title)
        var job1Result = response.Items.First(x => x.Title == "Senior Software Engineer");
        var job2Result = response.Items.First(x => x.Title == "Junior Developer");
        job1Result.RelevanceScore.Should().BeGreaterThan(job2Result.RelevanceScore);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var location = WorkLocation.Create("Hanoi", null, null, false).Value;
        var jobs = Enumerable.Range(1, 25)
            .Select(i => JobPosting.Create(
                $"Job {i}",
                $"Description {i}",
                Guid.NewGuid(),
                Guid.NewGuid(),
                JobType.FullTime,
                ExperienceLevel.Mid,
                location).Value)
            .ToList();

        _repository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<decimal?>(),
            Arg.Any<decimal?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns((jobs, 25));

        var query = new SearchJobPostingsQuery(Page: 2, PageSize: 10);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(10);
        result.Value.TotalCount.Should().Be(25);
        result.Value.TotalPages.Should().Be(3);
        result.Value.Page.Should().Be(2);
    }

    [Fact]
    public async Task Handle_SortByRelevance_ShouldOrderCorrectly()
    {
        // Arrange
        var location = WorkLocation.Create("Hanoi", null, null, false).Value;
        
        var job1 = JobPosting.Create(
            "Backend Developer",
            "Build APIs",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            location).Value;

        var job2 = JobPosting.Create(
            "Frontend Developer Backend",
            "Build UI",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            location).Value;

        _repository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<decimal?>(),
            Arg.Any<decimal?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<JobPosting> { job1, job2 }, 2));

        var query = new SearchJobPostingsQuery(
            Keywords: "backend",
            SortBy: SearchSortBy.Relevance,
            SortDescending: true);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items[0].Title.Should().Be("Backend Developer");
    }

    [Fact]
    public async Task Handle_WithNoResults_ShouldReturnEmptyList()
    {
        // Arrange
        _repository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<decimal?>(),
            Arg.Any<decimal?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<JobPosting>(), 0));

        var query = new SearchJobPostingsQuery(Keywords: "nonexistent");

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldIncludeSearchMetadata()
    {
        // Arrange
        var location = WorkLocation.Create("Hanoi", null, null, false).Value;
        var job = JobPosting.Create(
            "Test Job",
            "Test Description",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            location).Value;

        _repository.SearchAsync(
            Arg.Any<string?>(),
            Arg.Any<Guid?>(),
            Arg.Any<JobType?>(),
            Arg.Any<ExperienceLevel?>(),
            Arg.Any<JobPostingStatus?>(),
            Arg.Any<string?>(),
            Arg.Any<bool?>(),
            Arg.Any<decimal?>(),
            Arg.Any<decimal?>(),
            Arg.Any<string?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<List<string>?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<DateTime?>(),
            Arg.Any<CancellationToken>())
            .Returns((new List<JobPosting> { job }, 1));

        var query = new SearchJobPostingsQuery(
            Keywords: "software",
            City: "Hanoi",
            JobType: JobType.FullTime);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var metadata = result.Value.Metadata;
        metadata.SearchKeywords.Should().Be("software");
        metadata.FiltersApplied.Should().Be(3);
        metadata.SearchDuration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}
