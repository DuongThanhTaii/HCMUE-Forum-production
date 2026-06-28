using FluentAssertions;
using Microsoft.Extensions.Options;
using UniHub.Identity.Infrastructure.Authentication;

namespace UniHub.Identity.Infrastructure.Tests.Authentication;

public class JwtSettingsValidationTests
{
    private readonly JwtSettingsValidation _validator;

    public JwtSettingsValidationTests()
    {
        _validator = new JwtSettingsValidation();
    }

    [Fact]
    public void Validate_WithValidSettings_ShouldReturnSuccess()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingThatMustBeAtLeast256BitsLong",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 15
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Should().Be(ValidateOptionsResult.Success);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidSecretKey_ShouldReturnFailure(string secretKey)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = secretKey!,
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 15
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("JWT SecretKey is required");
    }

    [Fact]
    public void Validate_WithShortSecretKey_ShouldReturnFailure()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "TooShort", // Less than 32 characters
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 15
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("JWT SecretKey must be at least 32 characters long");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidIssuer_ShouldReturnFailure(string issuer)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingThatMustBeAtLeast256BitsLong",
            Issuer = issuer!,
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 15
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("JWT Issuer is required");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithInvalidAudience_ShouldReturnFailure(string audience)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingThatMustBeAtLeast256BitsLong",
            Issuer = "TestIssuer",
            Audience = audience!,
            AccessTokenExpiryMinutes = 15
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("JWT Audience is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WithInvalidExpiryMinutes_ShouldReturnFailure(int expiryMinutes)
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "ThisIsAVeryLongSecretKeyForTestingThatMustBeAtLeast256BitsLong",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = expiryMinutes
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain("JWT AccessTokenExpiryMinutes must be greater than 0");
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllFailures()
    {
        // Arrange
        var settings = new JwtSettings
        {
            SecretKey = "", // Invalid
            Issuer = "", // Invalid
            Audience = "", // Invalid
            AccessTokenExpiryMinutes = 0 // Invalid
        };

        // Act
        var result = _validator.Validate(null, settings);

        // Assert
        result.Failed.Should().BeTrue();
        result.Failures.Should().HaveCount(4);
        result.Failures.Should().Contain("JWT SecretKey is required");
        result.Failures.Should().Contain("JWT Issuer is required");
        result.Failures.Should().Contain("JWT Audience is required");
        result.Failures.Should().Contain("JWT AccessTokenExpiryMinutes must be greater than 0");
    }
}