using FluentAssertions;
using UniHub.Identity.Domain.Users;
using RefreshTokenEntity = UniHub.Identity.Domain.Tokens.RefreshToken;

namespace UniHub.Identity.Domain.Tests.Tokens;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldCreateValidRefreshToken()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = "test-refresh-token-12345";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var ipAddress = "192.168.1.1";

        // Act
        var refreshToken = RefreshTokenEntity.Create(userId, token, expiresAt, ipAddress);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.UserId.Should().Be(userId);
        refreshToken.Token.Should().Be(token);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.CreatedByIp.Should().Be(ipAddress);
        refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        refreshToken.IsActive.Should().BeTrue();
        refreshToken.IsExpired.Should().BeFalse();
        refreshToken.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void Create_WithoutIpAddress_ShouldCreateValidRefreshToken()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = "test-refresh-token-12345";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = RefreshTokenEntity.Create(userId, token, expiresAt);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.CreatedByIp.Should().BeNull();
        refreshToken.IsActive.Should().BeTrue();
    }

    [Fact]
    public void IsExpired_WhenTokenExpired_ShouldReturnTrue()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = "test-refresh-token-12345";
        var expiresAt = DateTime.UtcNow.AddSeconds(-1); // Expired 1 second ago

        // Act
        var refreshToken = RefreshTokenEntity.Create(userId, token, expiresAt);

        // Assert
        refreshToken.IsExpired.Should().BeTrue();
        refreshToken.IsActive.Should().BeFalse();
    }

    [Fact]
    public void IsExpired_WhenTokenNotExpired_ShouldReturnFalse()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = "test-refresh-token-12345";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = RefreshTokenEntity.Create(userId, token, expiresAt);

        // Assert
        refreshToken.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void Revoke_ShouldMarkTokenAsRevoked()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = "test-refresh-token-12345";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = RefreshTokenEntity.Create(userId, token, expiresAt);
        var revokedByIp = "192.168.1.100";
        var reason = "User logout";
        var replacedByToken = "new-token-67890";

        // Act
        refreshToken.Revoke(revokedByIp, reason, replacedByToken);

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.IsActive.Should().BeFalse();
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        refreshToken.RevokedByIp.Should().Be(revokedByIp);
        refreshToken.RevokeReason.Should().Be(reason);
        refreshToken.ReplacedByToken.Should().Be(replacedByToken);
    }

    [Fact]
    public void Revoke_WithMinimalParameters_ShouldMarkTokenAsRevoked()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = "test-refresh-token-12345";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = RefreshTokenEntity.Create(userId, token, expiresAt);

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.IsActive.Should().BeFalse();
        refreshToken.RevokedAt.Should().NotBeNull();
        refreshToken.RevokedByIp.Should().BeNull();
        refreshToken.RevokeReason.Should().BeNull();
        refreshToken.ReplacedByToken.Should().BeNull();
    }

    [Fact]
    public void IsActive_WhenTokenExpiredAndRevoked_ShouldReturnFalse()
    {
        // Arrange
        var userId = UserId.CreateUnique();
        var token = "test-refresh-token-12345";
        var expiresAt = DateTime.UtcNow.AddSeconds(-1); // Expired
        var refreshToken = RefreshTokenEntity.Create(userId, token, expiresAt);

        // Act
        refreshToken.Revoke();

        // Assert
        refreshToken.IsExpired.Should().BeTrue();
        refreshToken.IsRevoked.Should().BeTrue();
        refreshToken.IsActive.Should().BeFalse();
    }
}
