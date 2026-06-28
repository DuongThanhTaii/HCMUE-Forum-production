using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Commands.Login;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Tokens;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Tests.Commands.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAzureGuestInvitationService _azureGuestInvitationService;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtService = Substitute.For<IJwtService>();
        _refreshTokenRepository = Substitute.For<IRefreshTokenRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _azureGuestInvitationService = Substitute.For<IAzureGuestInvitationService>();
        _logger = Substitute.For<ILogger<LoginCommandHandler>>();
        _roleRepository.GetByIdsAsync(Arg.Any<IEnumerable<RoleId>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _handler = new LoginCommandHandler(
            _userRepository,
            _passwordHasher,
            _jwtService,
            _refreshTokenRepository,
            _roleRepository,
            _azureGuestInvitationService,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessWithTokens()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var profile = UserProfile.Create("Test", "User").Value;
        var user = User.Create(email, "hashedPassword", profile).Value;
        var refreshToken = RefreshToken.Create(user.Id, "refresh_token_123", DateTime.UtcNow.AddDays(7), "192.168.1.1");

        var command = new LoginCommand("test@example.com", "password123");

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.PasswordHash)
            .Returns(true);
        _jwtService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>?>())
            .Returns(Result.Success("access_token"));
        _jwtService.GenerateRefreshToken(Arg.Any<UserId>())
            .Returns(refreshToken);
        _jwtService.AccessTokenExpiry.Returns(TimeSpan.FromMinutes(15));
        _jwtService.RefreshTokenExpiry.Returns(TimeSpan.FromDays(7));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("test@example.com");
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        await _userRepository.Received(1).GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).VerifyPassword(command.Password, user.PasswordHash);
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand("invalid-email", "password123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Login.InvalidCredentials");
        await _userRepository.DidNotReceive().GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand("nonexistent@example.com", "password123");

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Login.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var profile = UserProfile.Create("Test", "User").Value;
        var user = User.Create(email, "hashedPassword", profile).Value;

        var command = new LoginCommand("test@example.com", "wrongpassword");

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.PasswordHash)
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Login.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var profile = UserProfile.Create("Test", "User").Value;
        var user = User.Create(email, "hashedPassword", profile).Value;
        user.ChangeStatus(UserStatus.Suspended); // Make user inactive

        var command = new LoginCommand("test@example.com", "password123");

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.PasswordHash)
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Login.UserNotActive");
    }

    [Fact]
    public async Task Handle_ShouldGenerateBothAccessAndRefreshTokens()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var profile = UserProfile.Create("Test", "User").Value;
        var user = User.Create(email, "hashedPassword", profile).Value;
        var refreshToken = RefreshToken.Create(user.Id, "refresh_token_456", DateTime.UtcNow.AddDays(7), "192.168.1.1");

        var command = new LoginCommand("test@example.com", "password123");

        _userRepository.GetByEmailAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.VerifyPassword(command.Password, user.PasswordHash)
            .Returns(true);
        _jwtService.GenerateAccessToken(user, Arg.Any<IEnumerable<string>?>())
            .Returns(Result.Success("access_token_123"));
        _jwtService.GenerateRefreshToken(Arg.Any<UserId>())
            .Returns(refreshToken);
        _jwtService.AccessTokenExpiry.Returns(TimeSpan.FromMinutes(15));
        _jwtService.RefreshTokenExpiry.Returns(TimeSpan.FromDays(7));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token_123");
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        _jwtService.Received(1).GenerateAccessToken(user, Arg.Any<IEnumerable<string>?>());
        _jwtService.Received(1).GenerateRefreshToken(Arg.Any<UserId>());
        await _refreshTokenRepository.Received(1).AddAsync(Arg.Any<RefreshToken>(), Arg.Any<CancellationToken>());
    }
}
