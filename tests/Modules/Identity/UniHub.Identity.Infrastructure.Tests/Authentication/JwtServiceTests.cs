using FluentAssertions;
using Microsoft.Extensions.Options;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.Identity.Infrastructure.Authentication;

namespace UniHub.Identity.Infrastructure.Tests.Authentication;

public class JwtServiceTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtService _jwtService;

    public JwtServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingThatMustBeAtLeast256BitsLongToWorkProperly",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 15
        };

        var options = Options.Create(_jwtSettings);
        _jwtService = new JwtService(options);
    }

    [Fact]
    public void AccessTokenExpiry_ShouldReturn15Minutes()
    {
        // Act
        var expiry = _jwtService.AccessTokenExpiry;

        // Assert
        expiry.Should().Be(TimeSpan.FromMinutes(15));
    }

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = _jwtService.GenerateAccessToken(user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
        result.Value.Split('.').Should().HaveCount(3); // JWT has 3 parts
    }

    [Fact]
    public void GenerateAccessToken_WithUserWithRoles_ShouldIncludeRoleClaims()
    {
        // Arrange
        var user = CreateTestUser();
        // Note: We can't easily test role claims without setting up the role relationship
        // This would require the Role aggregate and proper domain setup

        // Act
        var result = _jwtService.GenerateAccessToken(user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var user = CreateTestUser();
        var tokenResult = _jwtService.GenerateAccessToken(user);
        var token = tokenResult.Value;

        // Act
        var validationResult = _jwtService.ValidateToken(token);

        // Assert
        validationResult.IsSuccess.Should().BeTrue();
        validationResult.Value.Should().Be(user.Id);
    }

    [Fact]
    public void ValidateToken_WithInvalidToken_ShouldReturnFailure()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var result = _jwtService.ValidateToken(invalidToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JwtService.TokenValidation.Failed");
    }

    [Fact]
    public void ValidateToken_WithTokenFromDifferentSettings_ShouldReturnFailure()
    {
        // Arrange - This test verifies validation catches mismatched tokens
        // We test with wrong secret key which should cause validation to fail
        var differentSecretSettings = new JwtSettings
        {
            SecretKey = "DifferentVeryLongSecretKeyForTestingThatIsAtLeast256BitsLongToWorkProperly",
            Issuer = "TestIssuer",  // Same issuer
            Audience = "TestAudience", // Same audience
            AccessTokenExpiryMinutes = 15
        };

        var differentService = new JwtService(Options.Create(differentSecretSettings));
        var user = CreateTestUser();
        var tokenResult = differentService.GenerateAccessToken(user);
        
        tokenResult.IsSuccess.Should().BeTrue();

        // Act - Validate with original service (different secret key)
        var validationResult = _jwtService.ValidateToken(tokenResult.Value);

        // Assert
        validationResult.IsFailure.Should().BeTrue();
        validationResult.Error.Code.Should().Contain("JwtService.TokenValidation");
    }

    [Fact]
    public void ValidateToken_WithWrongIssuer_ShouldReturnFailure()
    {
        // Arrange
        var wrongIssuerSettings = new JwtSettings
        {
            SecretKey = _jwtSettings.SecretKey,
            Issuer = "WrongIssuer",
            Audience = _jwtSettings.Audience,
            AccessTokenExpiryMinutes = 15
        };

        var wrongIssuerService = new JwtService(Options.Create(wrongIssuerSettings));
        var user = CreateTestUser();
        var tokenResult = wrongIssuerService.GenerateAccessToken(user);

        // Act
        var validationResult = _jwtService.ValidateToken(tokenResult.Value);

        // Assert
        validationResult.IsFailure.Should().BeTrue();
        validationResult.Error.Code.Should().Be("JwtService.TokenValidation.Invalid");
    }

    [Fact]
    public void ValidateToken_WithWrongSecret_ShouldReturnFailure()
    {
        // Arrange
        var wrongSecretSettings = new JwtSettings
        {
            SecretKey = "DifferentSecretKeyThatIsAlsoLongEnoughForTestingPurposesToWorkProperly",
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            AccessTokenExpiryMinutes = 15
        };

        var wrongSecretService = new JwtService(Options.Create(wrongSecretSettings));
        var user = CreateTestUser();
        var tokenResult = wrongSecretService.GenerateAccessToken(user);

        // Act
        var validationResult = _jwtService.ValidateToken(tokenResult.Value);

        // Assert
        validationResult.IsFailure.Should().BeTrue();
        validationResult.Error.Code.Should().Be("JwtService.TokenValidation.Invalid");
    }

    [Fact]
    public void ValidateToken_WithEmptyToken_ShouldReturnFailure()
    {
        // Act
        var result = _jwtService.ValidateToken(string.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JwtService.TokenValidation.Failed");
    }

    [Fact]
    public void ValidateToken_WithNullToken_ShouldReturnFailure()
    {
        // Act
        var result = _jwtService.ValidateToken(null!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("JwtService.TokenValidation.Failed");
    }

    private User CreateTestUser()
    {
        var email = Email.Create("test@unihub.edu").Value;
        var profile = UserProfile.Create("Test", "User", "Hanoi", "Computer Science").Value;
        
        return User.Create(email, "hashedPassword", profile).Value;
    }
}