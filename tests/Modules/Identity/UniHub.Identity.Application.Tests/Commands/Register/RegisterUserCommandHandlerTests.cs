using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Commands.Register;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Application.Tests.Commands.Register;

public sealed class RegisterUserCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAzureGuestInvitationService _azureGuestInvitationService;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly RegisterUserCommandHandler _handler;

    public RegisterUserCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _roleRepository = Substitute.For<IRoleRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _azureGuestInvitationService = Substitute.For<IAzureGuestInvitationService>();
        _logger = Substitute.For<ILogger<RegisterUserCommandHandler>>();
        _handler = new RegisterUserCommandHandler(
            _userRepository,
            _roleRepository,
            _passwordHasher,
            _azureGuestInvitationService,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldRegisterUser()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: "Test User",
            Bio: "Test bio",
            AvatarUrl: "https://example.com/avatar.jpg");

        _userRepository.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _passwordHasher.HashPassword(command.Password)
            .Returns("hashed_password");

        var studentRole = Role.Create("Student", "Default student role").Value;
        _roleRepository.GetByNameAsync("Student", Arg.Any<CancellationToken>())
            .Returns(studentRole);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        await _userRepository.Received(1).IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
        _passwordHasher.Received(1).HashPassword(command.Password);
        await _userRepository.Received(1).AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "invalid-email",
            Password: "Test@1234",
            FullName: "Test User");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Contain("Email");

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "existing@example.com",
            Password: "Test@1234",
            FullName: "Test User");

        _userRepository.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(UserErrors.EmailAlreadyExists.Code);

        await _userRepository.Received(1).IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithInvalidFullName_ShouldReturnFailure()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: ""); // Empty full name

        _userRepository.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();

        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: "Test User");

        _userRepository.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _passwordHasher.HashPassword(command.Password)
            .Returns("hashed_password");

        var studentRole = Role.Create("Student", "Default student role").Value;
        _roleRepository.GetByNameAsync("Student", Arg.Any<CancellationToken>())
            .Returns(studentRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasher.Received(1).HashPassword(command.Password);
    }

    [Fact]
    public async Task Handle_ShouldAssignDefaultStudentRole()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: "Test User");

        _userRepository.IsEmailUniqueAsync(Arg.Any<Email>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _passwordHasher.HashPassword(command.Password)
            .Returns("hashed_password");

        var studentRole = Role.Create("Student", "Default student role").Value;
        _roleRepository.GetByNameAsync("Student", Arg.Any<CancellationToken>())
            .Returns(studentRole);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _roleRepository.Received(1).GetByNameAsync("Student", Arg.Any<CancellationToken>());
    }
}
