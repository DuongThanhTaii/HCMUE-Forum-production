using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.Recruiters.GetRecruitersForCompany;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters;

namespace UniHub.Career.Application.Tests.Queries.Recruiters;

public class GetRecruitersForCompanyQueryHandlerTests
{
    private readonly IRecruiterRepository _recruiterRepository;
    private readonly GetRecruitersForCompanyQueryHandler _handler;

    public GetRecruitersForCompanyQueryHandlerTests()
    {
        _recruiterRepository = Substitute.For<IRecruiterRepository>();
        _handler = new GetRecruitersForCompanyQueryHandler(_recruiterRepository);
    }

    [Fact]
    public async Task Handle_WithActiveOnly_ShouldReturnOnlyActiveRecruiters()
    {
        // Arrange
        var companyId = CompanyId.CreateUnique();
        var recruiter1 = Recruiter.Add(Guid.NewGuid(), companyId, RecruiterPermissions.Default(), Guid.NewGuid()).Value;
        var recruiter2 = Recruiter.Add(Guid.NewGuid(), companyId, RecruiterPermissions.Default(), Guid.NewGuid()).Value;

        _recruiterRepository.GetActiveByCompanyAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(new List<Recruiter> { recruiter1, recruiter2 });

        var query = new GetRecruitersForCompanyQuery(companyId.Value, ActiveOnly: true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Recruiters.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);

        await _recruiterRepository.Received(1).GetActiveByCompanyAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>());
        await _recruiterRepository.DidNotReceive().GetByCompanyAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithoutActiveOnly_ShouldReturnAllRecruiters()
    {
        // Arrange
        var companyId = CompanyId.CreateUnique();
        var recruiter1 = Recruiter.Add(Guid.NewGuid(), companyId, RecruiterPermissions.Default(), Guid.NewGuid()).Value;
        var recruiter2 = Recruiter.Add(Guid.NewGuid(), companyId, RecruiterPermissions.Default(), Guid.NewGuid()).Value;
        recruiter2.Deactivate(Guid.NewGuid());

        _recruiterRepository.GetByCompanyAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(new List<Recruiter> { recruiter1, recruiter2 });

        var query = new GetRecruitersForCompanyQuery(companyId.Value, ActiveOnly: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Recruiters.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.Recruiters.Should().Contain(r => r.Status == "Active");
        result.Value.Recruiters.Should().Contain(r => r.Status == "Inactive");

        await _recruiterRepository.Received(1).GetByCompanyAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>());
        await _recruiterRepository.DidNotReceive().GetActiveByCompanyAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoRecruiters_ShouldReturnEmptyList()
    {
        // Arrange
        var companyId = CompanyId.CreateUnique();

        _recruiterRepository.GetByCompanyAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(new List<Recruiter>());

        var query = new GetRecruitersForCompanyQuery(companyId.Value, ActiveOnly: false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Recruiters.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }
}
