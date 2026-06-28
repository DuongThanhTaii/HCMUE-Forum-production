using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.Companies.GetRecentApplications;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using Xunit;
using DomainApplication = UniHub.Career.Domain.Applications.Application;

namespace UniHub.Career.Application.Tests.Queries.Companies;

public class GetRecentApplicationsQueryHandlerTests
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly GetRecentApplicationsQueryHandler _handler;

    public GetRecentApplicationsQueryHandlerTests()
    {
        _companyRepository = Substitute.For<ICompanyRepository>();
        _applicationRepository = Substitute.For<IApplicationRepository>();
        _jobPostingRepository = Substitute.For<IJobPostingRepository>();
        _handler = new GetRecentApplicationsQueryHandler(
            _companyRepository,
            _applicationRepository,
            _jobPostingRepository);
    }

    [Fact]
    public async Task Handle_WithNonExistentCompany_ShouldReturnFailure()
    {
        // Arrange
        var companyId = Guid.NewGuid();

        _companyRepository.GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        var query = new GetRecentApplicationsQuery(companyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Company.NotFound");
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldCalculateTotalPages()
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

        _companyRepository.GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(company);

        _applicationRepository.GetByCompanyAsync(companyId, null, 2, 5, Arg.Any<CancellationToken>())
            .Returns((new List<DomainApplication>(), 12));

        var query = new GetRecentApplicationsQuery(companyId, 2, 5);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalPages.Should().Be(3); // 12 items / 5 per page = 3 pages
    }

    [Fact]
    public async Task Handle_WhenNoApplications_ShouldReturnEmptyList()
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

        _companyRepository.GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(company);

        _applicationRepository.GetByCompanyAsync(companyId, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<DomainApplication>(), 0));

        var query = new GetRecentApplicationsQuery(companyId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
