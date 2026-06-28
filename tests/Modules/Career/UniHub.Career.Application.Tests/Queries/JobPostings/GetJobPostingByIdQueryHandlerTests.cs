using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.JobPostings.GetJobPostingById;
using UniHub.Career.Domain.JobPostings;
using Xunit;

namespace UniHub.Career.Application.Tests.Queries.JobPostings;

public class GetJobPostingByIdQueryHandlerTests
{
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly GetJobPostingByIdQueryHandler _handler;

    public GetJobPostingByIdQueryHandlerTests()
    {
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _handler = new GetJobPostingByIdQueryHandler(_jobPostingRepository);
    }

    [Fact]
    public async Task Handle_WithExistingJobPosting_ShouldReturnJobPosting()
    {
        // Arrange
        var jobPosting = JobPosting.Create(
            "Full Stack Developer",
            "Work on both frontend and backend.",
            Guid.NewGuid(),
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Senior,
            WorkLocation.Create("Ho Chi Minh City", "District 3").Value,
            SalaryRange.Create(2000m, 3500m, "USD", "month").Value,
            DateTime.UtcNow.AddMonths(2)).Value;

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var query = new GetJobPostingByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be("Full Stack Developer");
        result.Value.JobType.Should().Be("FullTime");
        result.Value.ExperienceLevel.Should().Be("Senior");
        result.Value.Location.City.Should().Be("Ho Chi Minh City");
        result.Value.Location.District.Should().Be("District 3");
        result.Value.Salary.Should().NotBeNull();
        result.Value.Salary!.MinAmount.Should().Be(2000m);
        result.Value.Salary.MaxAmount.Should().Be(3500m);
    }

    [Fact]
    public async Task Handle_WithNonExistentJobPosting_ShouldReturnFailure()
    {
        // Arrange
        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns((JobPosting?)null);

        var query = new GetJobPostingByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JobPosting.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var postedBy = Guid.NewGuid();
        var jobPosting = JobPosting.Create(
            "iOS Developer",
            "Build iOS applications.",
            companyId,
            postedBy,
            JobType.Remote,
            ExperienceLevel.Mid,
            WorkLocation.Create("Ha Noi", isRemote: true).Value,
            null,
            null).Value;

        jobPosting.Publish();

        _jobPostingRepository.GetByIdAsync(
            Arg.Any<JobPostingId>(),
            Arg.Any<CancellationToken>())
            .Returns(jobPosting);

        var query = new GetJobPostingByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CompanyId.Should().Be(companyId);
        result.Value.PostedBy.Should().Be(postedBy);
        result.Value.Status.Should().Be("Published");
        result.Value.PublishedAt.Should().NotBeNull();
        result.Value.Location.IsRemote.Should().BeTrue();
    }
}
