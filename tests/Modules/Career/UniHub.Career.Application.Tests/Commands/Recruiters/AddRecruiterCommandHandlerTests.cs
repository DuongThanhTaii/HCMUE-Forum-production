using FluentAssertions;
using NSubstitute;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.Recruiters.AddRecruiter;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters;

namespace UniHub.Career.Application.Tests.Commands.Recruiters;

public class AddRecruiterCommandHandlerTests
{
    private readonly IRecruiterRepository _recruiterRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly AddRecruiterCommandHandler _handler;

    public AddRecruiterCommandHandlerTests()
    {
        _recruiterRepository = Substitute.For<IRecruiterRepository>();
        _companyRepository = Substitute.For<ICompanyRepository>();
        _handler = new AddRecruiterCommandHandler(_recruiterRepository, _companyRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldAddRecruiter()
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

        _recruiterRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new AddRecruiterCommand(
            UserId: Guid.NewGuid(),
            CompanyId: companyId,
            CanManageJobPostings: true,
            CanReviewApplications: true,
            CanUpdateApplicationStatus: true,
            CanInviteRecruiters: false,
            AddedBy: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().Be(command.UserId);
        result.Value.CompanyId.Should().Be(command.CompanyId);
        result.Value.Status.Should().Be("Active");
        result.Value.Permissions.CanManageJobPostings.Should().BeTrue();
        result.Value.Permissions.CanInviteRecruiters.Should().BeFalse();

        await _recruiterRepository.Received(1).AddAsync(Arg.Any<Recruiter>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentCompany_ShouldReturnFailure()
    {
        // Arrange
        _companyRepository.GetByIdAsync(Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns((Company?)null);

        var command = new AddRecruiterCommand(
            UserId: Guid.NewGuid(),
            CompanyId: Guid.NewGuid(),
            AddedBy: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Company.NotFound");

        await _recruiterRepository.DidNotReceive().AddAsync(Arg.Any<Recruiter>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserAlreadyRecruiter_ShouldReturnFailure()
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

        _recruiterRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new AddRecruiterCommand(
            UserId: Guid.NewGuid(),
            CompanyId: companyId,
            AddedBy: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RecruiterErrors.AlreadyExists);

        await _recruiterRepository.DidNotReceive().AddAsync(Arg.Any<Recruiter>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNoPermissions_ShouldReturnFailure()
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

        _recruiterRepository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<CompanyId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new AddRecruiterCommand(
            UserId: Guid.NewGuid(),
            CompanyId: companyId,
            CanManageJobPostings: false,
            CanReviewApplications: false,
            CanUpdateApplicationStatus: false,
            CanInviteRecruiters: false,
            AddedBy: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RecruiterErrors.NoPermissionsGranted);

        await _recruiterRepository.DidNotReceive().AddAsync(Arg.Any<Recruiter>(), Arg.Any<CancellationToken>());
    }
}
