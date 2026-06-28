using FluentAssertions;
using UniHub.Identity.Application.Commands.Register;

namespace UniHub.Identity.Application.Tests.Commands.Register;

public sealed class RegisterUserCommandValidatorTests
{
    private readonly RegisterUserCommandValidator _validator;

    public RegisterUserCommandValidatorTests()
    {
        _validator = new RegisterUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: "Test User",
            Bio: "Test bio",
            AvatarUrl: "https://example.com/avatar.jpg");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: email,
            Password: "Test@1234",
            FullName: "Test User");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: email,
            Password: "Test@1234",
            FullName: "Test User");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: password,
            FullName: "Test User");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Theory]
    [InlineData("short")] // Too short
    [InlineData("nouppercaseletter1@")] // No uppercase
    [InlineData("NOLOWERCASELETTER1@")] // No lowercase
    [InlineData("NoDigitHere@")] // No digit
    [InlineData("NoSpecialChar1")] // No special character
    public void Validate_WithWeakPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: password,
            FullName: "Test User");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Password));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyFullName_ShouldFail(string fullName)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: fullName);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.FullName));
    }

    [Theory]
    [InlineData("A")] // Too short (1 character)
    [InlineData("AB")] // Just 2 characters but should need at least 2
    public void Validate_WithTooShortFullName_ShouldFailOrPass(string fullName)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: fullName);

        // Act
        var result = _validator.Validate(command);

        // Assert - "A" should fail, "AB" should pass (minimum 2 characters)
        if (fullName.Length < 2)
        {
            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.FullName));
        }
        else
        {
            result.IsValid.Should().BeTrue();
        }
    }

    [Fact]
    public void Validate_WithTooLongBio_ShouldFail()
    {
        // Arrange
        var longBio = new string('a', 501); // 501 characters
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: "Test User",
            Bio: longBio);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.Bio));
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/avatar.jpg")] // FTP not allowed
    public void Validate_WithInvalidAvatarUrl_ShouldFail(string avatarUrl)
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: "Test User",
            AvatarUrl: avatarUrl);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterUserCommand.AvatarUrl));
    }

    [Fact]
    public void Validate_WithNullOptionalFields_ShouldPass()
    {
        // Arrange
        var command = new RegisterUserCommand(
            Email: "test@example.com",
            Password: "Test@1234",
            FullName: "Test User",
            Bio: null,
            AvatarUrl: null);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
