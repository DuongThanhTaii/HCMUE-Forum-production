using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.Companies.GetCompanyStatistics;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using Xunit;
using DomainApplication = UniHub.Career.Domain.Applications.Application;

namespace UniHub.Career.Application.Tests.Queries.Companies;

public class GetCompanyStatisticsQueryHandlerTests
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly GetCompanyStatisticsQueryHandler _handler;

    public GetCompanyStatisticsQueryHandlerTests()
    {
        _companyRepository = Substitute.For<ICompanyRepository>();
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _handler = new GetCompanyStatisticsQueryHandler(
            _companyRepository,
            _jobPostingRepository,
            _applicationRepository);
    }

    [Fact]
    public async Task Handle_WithValidCompanyId_ShouldReturnStatistics()
    {
        // Arrange
        var companyId = Guid.NewGuid();

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

        var jobPosting1 = JobPosting.Create(
            "Software Engineer",
            "Description",
            companyId,
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Mid,
            WorkLocation.Create("Hanoi", null, null, false).Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;
        jobPosting1.Publish();

        var jobPosting2 = JobPosting.Create(
            "Frontend Developer",
            "Description",
            companyId,
            Guid.NewGuid(),
            JobType.PartTime,
            ExperienceLevel.Junior,
            WorkLocation.Create("Ho Chi Minh", null, null, false).Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;

        var application1 = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        var application2 = DomainApplication.Submit(
            JobPostingId.Create(Guid.NewGuid()),
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;
        application2.MoveToReviewing(Guid.NewGuid());

        _companyRepository.GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(company);

        _jobPostingRepository.GetByCompanyAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<JobPosting> { jobPosting1, jobPosting2 });

        _applicationRepository.GetByCompanyAsync(companyId, null, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<DomainApplication> { application1, application2 }, 2));

        var query = new GetCompanyStatisticsQuery(companyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CompanyName.Should().Be("Tech Corp");
        result.Value.Overview.TotalJobPostings.Should().Be(2);
        result.Value.Overview.ActiveJobPostings.Should().Be(1);
        result.Value.Overview.TotalApplications.Should().Be(2);
        result.Value.JobPostings.Draft.Should().Be(1);
        result.Value.JobPostings.Published.Should().Be(1);
        result.Value.Applications.Pending.Should().Be(1);
        result.Value.Applications.Reviewing.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithNonExistentCompany_ShouldReturnFailure()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        _companyRepository.GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        var query = new GetCompanyStatisticsQuery(companyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Company.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldCalculateAcceptanceAndRejectionRates()
    {
        // Arrange
        var companyId = Guid.NewGuid();

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

        var jobPostingId = JobPostingId.Create(Guid.NewGuid());

        var application1 = DomainApplication.Submit(
            jobPostingId,
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;
        application1.Offer(Guid.NewGuid());
        application1.Accept(application1.ApplicantId);

        var application2 = DomainApplication.Submit(
            jobPostingId,
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;
        application2.Reject(Guid.NewGuid(), "Not suitable");

        var application3 = DomainApplication.Submit(
            jobPostingId,
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;
        application3.Offer(Guid.NewGuid());
        application3.Accept(application3.ApplicantId);

        var application4 = DomainApplication.Submit(
            jobPostingId,
            Guid.NewGuid(),
            Resume.Create("resume.pdf", "https://example.com/resume.pdf", 1024000, "application/pdf").Value).Value;

        _companyRepository.GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(company);

        _jobPostingRepository.GetByCompanyAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<JobPosting>());

        _applicationRepository.GetByCompanyAsync(companyId, null, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<DomainApplication> { application1, application2, application3, application4 }, 4));

        var query = new GetCompanyStatisticsQuery(companyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Applications.Accepted.Should().Be(2);
        result.Value.Applications.Rejected.Should().Be(1);
        result.Value.Applications.Pending.Should().Be(1);
        result.Value.Applications.AcceptanceRate.Should().Be(50.0); // 2/4 = 50%
        result.Value.Applications.RejectionRate.Should().Be(25.0);  // 1/4 = 25%
    }

    [Fact]
    public async Task Handle_ShouldReturnTopPerformingJobs()
    {
        // Arrange
        var companyId = Guid.NewGuid();

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

        var job1 = JobPosting.Create(
            "Senior Engineer",
            "Description",
            companyId,
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Senior,
            WorkLocation.Create("Hanoi", null, null, false).Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;
        job1.Publish();
        job1.IncrementApplicationCount();
        job1.IncrementApplicationCount();
        job1.IncrementApplicationCount();

        var job2 = JobPosting.Create(
            "Junior Developer",
            "Description",
            companyId,
            Guid.NewGuid(),
            JobType.FullTime,
            ExperienceLevel.Junior,
            WorkLocation.Create("Hanoi", null, null, false).Value,
            null,
            DateTime.UtcNow.AddMonths(1)).Value;
        job2.Publish();
        job2.IncrementApplicationCount();

        _companyRepository.GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(company);

        _jobPostingRepository.GetByCompanyAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<JobPosting> { job1, job2 });

        _applicationRepository.GetByCompanyAsync(companyId, null, Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<DomainApplication>(), 0));

        var query = new GetCompanyStatisticsQuery(companyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TopPerformingJobs.Should().HaveCount(2);
        result.Value.TopPerformingJobs[0].Title.Should().Be("Senior Engineer");
        result.Value.TopPerformingJobs[0].ApplicationCount.Should().Be(3);
        result.Value.TopPerformingJobs[1].Title.Should().Be("Junior Developer");
        result.Value.TopPerformingJobs[1].ApplicationCount.Should().Be(1);
    }
}
