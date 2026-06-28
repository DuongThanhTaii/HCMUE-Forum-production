using FluentAssertions;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Domain.Tests.Users.ValueObjects;

public class OfficialBadgeTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        const BadgeType type = BadgeType.Department;
        const string name = "Computer Science Department";
        const string verifiedBy = "admin@university.edu";

        // Act
        var result = OfficialBadge.Create(type, name, verifiedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(type);
        result.Value.Name.Should().Be(name);
        result.Value.VerifiedBy.Should().Be(verifiedBy);
        result.Value.VerifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyName_ShouldFail(string? name)
    {
        // Act
        var result = OfficialBadge.Create(BadgeType.Department, name!, "admin@test.com");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OfficialBadge.Name.Empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyVerifiedBy_ShouldFail(string? verifiedBy)
    {
        // Act
        var result = OfficialBadge.Create(BadgeType.Department, "Test Department", verifiedBy!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OfficialBadge.VerifiedBy.Empty");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldFail()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var result = OfficialBadge.Create(BadgeType.Department, longName, "admin@test.com");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OfficialBadge.Name.TooLong");
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldFail()
    {
        // Arrange
        var longDescription = new string('A', 201);

        // Act
        var result = OfficialBadge.Create(BadgeType.Department, "Test", "admin@test.com", longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("OfficialBadge.Description.TooLong");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Act
        var result = OfficialBadge.Create(BadgeType.Department, "  Test  ", "  admin@test.com  ", "  Description  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test");
        result.Value.VerifiedBy.Should().Be("admin@test.com");
        result.Value.Description.Should().Be("Description");
    }

    [Theory]
    [InlineData(BadgeType.Department, "🔵")]
    [InlineData(BadgeType.Club, "🟢")]
    [InlineData(BadgeType.BoardOfDirectors, "🟡")]
    [InlineData(BadgeType.Faculty, "🟣")]
    [InlineData(BadgeType.Company, "🟠")]
    public void DisplayText_ShouldReturnCorrectFormat(BadgeType type, string expectedEmoji)
    {
        // Arrange
        const string name = "Test Badge";
        var badge = OfficialBadge.Create(type, name, "admin@test.com").Value;

        // Act
        var displayText = badge.DisplayText;

        // Assert
        displayText.Should().Be($"{expectedEmoji} {name}");
    }

    [Fact]
    public void TwoBadgesWithSameData_ShouldBeEqual()
    {
        // Arrange
        var badge1 = OfficialBadge.Create(BadgeType.Department, "Test", "admin@test.com").Value;
        var badge2 = OfficialBadge.Create(BadgeType.Department, "Test", "admin@test.com").Value;

        // Act & Assert
        badge1.Should().Be(badge2);
    }
}