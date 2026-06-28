using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.Companies.RegisterCompany;
using UniHub.Career.Domain.Companies;

namespace UniHub.Career.Application.Tests.Commands.Companies;

public class RegisterCompanyCommandHandlerTests
{
    private readonly ICompanyRepository _companyRepository;
    private readonly RegisterCompanyCommandHandler _handler;

    public RegisterCompanyCommandHandlerTests()
    {
        _companyRepository = Substitute.For<ICompanyRepository>();
        _handler = new RegisterCompanyCommandHandler(_companyRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterCompany()
    {
        // Arrange
        var command = new RegisterCompanyCommand(
            Name: "Tech Solutions Inc",
            Description: "A leading technology company providing innovative solutions.",
            Industry: Industry.Technology,
            Size: CompanySize.Medium,
            Email: "contact@techsolutions.com",
            Phone: "+84123456789",
            Address: "123 Main St, Ho Chi Minh City",
            RegisteredBy: Guid.NewGuid(),
            Website: "https://techsolutions.com",
            LogoUrl: "https://cdn.example.com/logo.png",
            FoundedYear: 2015,
            LinkedIn: "https://linkedin.com/company/techsolutions",
            Facebook: null,
            Twitter: null,
            Instagram: null,
            YouTube: null);

        _companyRepository.IsNameUniqueAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(command.Name);
        result.Value.Description.Should().Be(command.Description);
        result.Value.Industry.Should().Be(Industry.Technology.ToString());
        result.Value.Size.Should().Be(CompanySize.Medium.ToString());
        result.Value.Status.Should().Be(CompanyStatus.Pending.ToString());
        result.Value.Website.Should().Be(command.Website);

        await _companyRepository.Received(1).AddAsync(
            Arg.Is<Company>(c => c.Name == command.Name),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCompanyCommand(
            Name: "Existing Company",
            Description: "Description",
            Industry: Industry.Technology,
            Size: CompanySize.Small,
            Email: "contact@existing.com",
            Phone: null,
            Address: null,
            RegisteredBy: Guid.NewGuid());

        _companyRepository.IsNameUniqueAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Company.NameNotUnique");

        await _companyRepository.DidNotReceive().AddAsync(
            Arg.Any<Company>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCompanyCommand(
            Name: "Tech Solutions Inc",
            Description: "Description",
            Industry: Industry.Technology,
            Size: CompanySize.Medium,
            Email: "invalid-email",
            Phone: null,
            Address: null,
            RegisteredBy: Guid.NewGuid());

        _companyRepository.IsNameUniqueAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("ContactInfo.InvalidEmail");

        await _companyRepository.DidNotReceive().AddAsync(
            Arg.Any<Company>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidWebsiteUrl_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCompanyCommand(
            Name: "Tech Solutions Inc",
            Description: "Description",
            Industry: Industry.Technology,
            Size: CompanySize.Medium,
            Email: "contact@techsolutions.com",
            Phone: null,
            Address: null,
            RegisteredBy: Guid.NewGuid(),
            Website: "not-a-url");

        _companyRepository.IsNameUniqueAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Company.InvalidWebsiteUrl");

        await _companyRepository.DidNotReceive().AddAsync(
            Arg.Any<Company>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidSocialLink_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterCompanyCommand(
            Name: "Tech Solutions Inc",
            Description: "Description",
            Industry: Industry.Technology,
            Size: CompanySize.Medium,
            Email: "contact@techsolutions.com",
            Phone: null,
            Address: null,
            RegisteredBy: Guid.NewGuid(),
            LinkedIn: "not-a-url");

        _companyRepository.IsNameUniqueAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SocialLinks.InvalidLinkedIn");

        await _companyRepository.DidNotReceive().AddAsync(
            Arg.Any<Company>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMinimalData_ShouldRegisterCompany()
    {
        // Arrange
        var command = new RegisterCompanyCommand(
            Name: "Startup Co",
            Description: "New startup",
            Industry: Industry.Technology,
            Size: CompanySize.Startup,
            Email: "contact@startup.co",
            Phone: null,
            Address: null,
            RegisteredBy: Guid.NewGuid());

        _companyRepository.IsNameUniqueAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Website.Should().BeNull();
        result.Value.LogoUrl.Should().BeNull();
        result.Value.FoundedYear.Should().BeNull();

        await _companyRepository.Received(1).AddAsync(
            Arg.Any<Company>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldMapResponseCorrectly()
    {
        // Arrange
        var registeredBy = Guid.NewGuid();
        var command = new RegisterCompanyCommand(
            Name: "Tech Corp",
            Description: "Tech company",
            Industry: Industry.Finance,
            Size: CompanySize.Large,
            Email: "info@techcorp.com",
            Phone: "+1234567890",
            Address: "456 Tech Ave",
            RegisteredBy: registeredBy,
            Website: "https://techcorp.com",
            LogoUrl: "https://logo.png",
            FoundedYear: 2010);

        _companyRepository.IsNameUniqueAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var response = result.Value;
        
        response.CompanyId.Should().NotBeEmpty();
        response.Name.Should().Be("Tech Corp");
        response.Description.Should().Be("Tech company");
        response.Industry.Should().Be("Finance");
        response.Size.Should().Be("Large");
        response.Status.Should().Be("Pending");
        response.ContactInfo.Should().NotBeNull();
        response.ContactInfo.Email.Should().Be("info@techcorp.com");
        response.Website.Should().Be("https://techcorp.com");
        response.LogoUrl.Should().Be("https://logo.png");
        response.FoundedYear.Should().Be(2010);
        response.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
