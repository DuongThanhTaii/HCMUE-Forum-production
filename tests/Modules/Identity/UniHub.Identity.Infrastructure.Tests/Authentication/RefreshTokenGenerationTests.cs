using FluentAssertions;
using Microsoft.Extensions.Options;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Infrastructure.Authentication;

namespace UniHub.Identity.Infrastructure.Tests.Authentication;

public sealed class RefreshTokenGenerationTests
{
    private readonly JwtService _jwtService;

    public RefreshTokenGenerationTests()
    {
        var jwtSettings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatMustBeAtLeast256BitsLong",
            Issuer = "UniHub.Test",
            Audience = "UniHub.TestClient",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7
        };

        var options = Options.Create(jwtSettings);
        _jwtService = new JwtService(options);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldGenerateValidToken()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var ipAddress = "192.168.1.1";

        // Act
        var refreshToken = _jwtService.GenerateRefreshToken(userId, ipAddress);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.UserId.Should().Be(userId);
        refreshToken.Token.Should().NotBeNullOrWhiteSpace();
        refreshToken.Token.Length.Should().BeGreaterThan(40); // Base64 encoded 64 bytes
        refreshToken.CreatedByIp.Should().Be(ipAddress);
        refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(5));
        refreshToken.IsActive.Should().BeTrue();
        refreshToken.IsExpired.Should().BeFalse();
        refreshToken.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void GenerateRefreshToken_WithoutIpAddress_ShouldGenerateValidToken()
    {
        // Arrange
        var userId = UserId.CreateUnique();

        // Act
        var refreshToken = _jwtService.GenerateRefreshToken(userId);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.UserId.Should().Be(userId);
        refreshToken.CreatedByIp.Should().BeNull();
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void GenerateRefreshToken_MultipleCalls_ShouldGenerateUniqueTokens()
    {
        // Arrange
        var userId = UserId.CreateUnique();

        // Act
        var token1 = _jwtService.GenerateRefreshToken(userId);
        var token2 = _jwtService.GenerateRefreshToken(userId);
        var token3 = _jwtService.GenerateRefreshToken(userId);

        // Assert
        token1.Token.Should().NotBe(token2.Token);
        token1.Token.Should().NotBe(token3.Token);
        token2.Token.Should().NotBe(token3.Token);
    }

    [Fact]
    public void RefreshTokenExpiry_ShouldMatch7Days()
    {
        // Act
        var expiry = _jwtService.RefreshTokenExpiry;

        // Assert
        expiry.Should().Be(TimeSpan.FromDays(7));
    }

    [Fact]
    public void GenerateRefreshToken_ShouldUseConfiguredExpiry()
    {
        // Arrange
        var customSettings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForJWTTokenGenerationThatMustBeAtLeast256BitsLong",
            Issuer = "UniHub.Test",
            Audience = "UniHub.TestClient",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 30 // Custom 30 days
        };

        var customService = new JwtService(Options.Create(customSettings));
        var userId = UserId.CreateUnique();

        // Act
        var refreshToken = customService.GenerateRefreshToken(userId);

        // Assert
        refreshToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromSeconds(5));
        customService.RefreshTokenExpiry.Should().Be(TimeSpan.FromDays(30));
    }

    [Fact]
    public void GenerateRefreshToken_TokenShouldBeBase64Encoded()
    {
        // Arrange
        var userId = UserId.CreateUnique();

        // Act
        var refreshToken = _jwtService.GenerateRefreshToken(userId);

        // Assert - Should be able to decode from Base64
        var action = () => Convert.FromBase64String(refreshToken.Token);
        action.Should().NotThrow();
        
        var decoded = Convert.FromBase64String(refreshToken.Token);
        decoded.Length.Should().Be(64); // 64 random bytes
    }
}
