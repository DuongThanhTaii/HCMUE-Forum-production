using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Queries.Recruiters.IsUserRecruiter;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters;

namespace UniHub.Career.Application.Tests.Queries.Recruiters;

public class IsUserRecruiterQueryHandlerTests
{
    private readonly IRecruiterRepository _recruiterRepository;
    private readonly IsUserRecruiterQueryHandler _handler;

    public IsUserRecruiterQueryHandlerTests()
    {
        _recruiterRepository = Substitute.For<IRecruiterRepository>();
        _handler = new IsUserRecruiterQueryHandler(_recruiterRepository);
    }

    [Fact]
    public async Task Handle_WhenUserIsActiveRecruiter_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = CompanyId.CreateUnique();
        var recruiter = Recruiter.Add(userId, companyId, RecruiterPermissions.Admin(), Guid.NewGuid()).Value;

        _recruiterRepository.GetByUserAndCompanyAsync(userId, Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(recruiter);

        var query = new IsUserRecruiterQuery(userId, companyId.Value);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsRecruiter.Should().BeTrue();
        result.Value.IsActive.Should().BeTrue();
        result.Value.Permissions.Should().NotBeNull();
        result.Value.Permissions!.CanInviteRecruiters.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenUserIsInactiveRecruiter_ShouldReturnInactive()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = CompanyId.CreateUnique();
        var recruiter = Recruiter.Add(userId, companyId, RecruiterPermissions.Default(), Guid.NewGuid()).Value;
        recruiter.Deactivate(Guid.NewGuid());

        _recruiterRepository.GetByUserAndCompanyAsync(userId, Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(recruiter);

        var query = new IsUserRecruiterQuery(userId, companyId.Value);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsRecruiter.Should().BeTrue();
        result.Value.IsActive.Should().BeFalse();
        result.Value.Permissions.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WhenUserIsNotRecruiter_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var companyId = CompanyId.CreateUnique();

        _recruiterRepository.GetByUserAndCompanyAsync(userId, Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns((Recruiter?)null);

        var query = new IsUserRecruiterQuery(userId, companyId.Value);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsRecruiter.Should().BeFalse();
        result.Value.IsActive.Should().BeFalse();
        result.Value.Permissions.Should().BeNull();
    }
}
