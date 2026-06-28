using FluentAssertions;
using UniHub.Identity.Domain.Events;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Domain.Tests.Users;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        const string passwordHash = "hashedPassword";
        var profile = UserProfile.Create("John", "Doe").Value;

        // Act
        var result = User.Create(email, passwordHash, profile);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var user = result.Value;
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.Profile.Should().Be(profile);
        user.Status.Should().Be(UserStatus.Active);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.Badge.Should().BeNull();
        user.Roles.Should().BeEmpty();
        user.RefreshTokens.Should().BeEmpty();

        // Check domain event
        user.DomainEvents.Should().HaveCount(1);
        user.DomainEvents.First().Should().BeOfType<UserRegisteredEvent>();
        var domainEvent = (UserRegisteredEvent)user.DomainEvents.First();
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.Email.Should().Be(email);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyPasswordHash_ShouldFail(string? passwordHash)
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var profile = UserProfile.Create("John", "Doe").Value;

        // Act
        var result = User.Create(email, passwordHash!, profile);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.PasswordHash.Empty");
    }

    [Fact]
    public void UpdateProfile_ShouldSucceedAndRaiseDomainEvent()
    {
        // Arrange
        var user = CreateTestUser();
        var newProfile = UserProfile.Create("Jane", "Smith").Value;
        var originalUpdatedAt = user.UpdatedAt;

        // Act
        var result = user.UpdateProfile(newProfile);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Profile.Should().Be(newProfile);
        user.UpdatedAt.Should().BeAfter(originalUpdatedAt ?? DateTime.MinValue);

        user.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdatedEvent);
        var domainEvent = user.DomainEvents.OfType<UserProfileUpdatedEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.NewProfile.Should().Be(newProfile);
    }

    [Fact]
    public void ChangeStatus_WithDifferentStatus_ShouldSucceedAndRaiseDomainEvent()
    {
        // Arrange
        var user = CreateTestUser();
        const UserStatus newStatus = UserStatus.Suspended;

        // Act
        var result = user.ChangeStatus(newStatus);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Status.Should().Be(newStatus);

        user.DomainEvents.Should().ContainSingle(e => e is UserStatusChangedEvent);
        var domainEvent = user.DomainEvents.OfType<UserStatusChangedEvent>().Single();
        domainEvent.UserId.Should().Be(user.Id);
        domainEvent.OldStatus.Should().Be(UserStatus.Active);
        domainEvent.NewStatus.Should().Be(newStatus);
    }

    [Fact]
    public void ChangeStatus_WithSameStatus_ShouldSucceedWithoutEvent()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var result = user.ChangeStatus(UserStatus.Active);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.DomainEvents.Should().NotContain(e => e is UserStatusChangedEvent);
    }

    [Fact]
    public void AssignRole_WithNewRole_ShouldSucceed()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = RoleId.CreateUnique();

        // Act
        var result = user.AssignRole(roleId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Roles.Should().HaveCount(1);
        user.Roles.First().RoleId.Should().Be(roleId);

        user.DomainEvents.Should().ContainSingle(e => e is RoleAssignedEvent);
    }

    [Fact]
    public void AssignRole_WithExistingRole_ShouldFail()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = RoleId.CreateUnique();
        user.AssignRole(roleId);

        // Act
        var result = user.AssignRole(roleId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.RoleAlreadyAssigned");
        user.Roles.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveRole_WithExistingRole_ShouldSucceed()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId1 = RoleId.CreateUnique();
        var roleId2 = RoleId.CreateUnique();
        user.AssignRole(roleId1);
        user.AssignRole(roleId2);

        // Act
        var result = user.RemoveRole(roleId1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Roles.Should().HaveCount(1);
        user.Roles.First().RoleId.Should().Be(roleId2);
    }

    [Fact]
    public void RemoveRole_WithNonExistentRole_ShouldFail()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = RoleId.CreateUnique();

        // Act
        var result = user.RemoveRole(roleId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.RoleNotAssigned");
    }

    [Fact]
    public void RemoveRole_WithLastRole_ShouldFail()
    {
        // Arrange
        var user = CreateTestUser();
        var roleId = RoleId.CreateUnique();
        user.AssignRole(roleId);

        // Act
        var result = user.RemoveRole(roleId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.CannotDeleteLastRole");
    }

    [Fact]
    public void SetOfficialBadge_WithNoBadge_ShouldSucceed()
    {
        // Arrange
        var user = CreateTestUser();
        var badge = OfficialBadge.Create(BadgeType.Department, "CS Department", "admin@test.com").Value;

        // Act
        var result = user.SetOfficialBadge(badge);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Badge.Should().Be(badge);

        user.DomainEvents.Should().ContainSingle(e => e is OfficialBadgeAssignedEvent);
    }

    [Fact]
    public void SetOfficialBadge_WithExistingBadge_ShouldFail()
    {
        // Arrange
        var user = CreateTestUser();
        var badge1 = OfficialBadge.Create(BadgeType.Department, "CS Department", "admin@test.com").Value;
        var badge2 = OfficialBadge.Create(BadgeType.Faculty, "Professor", "admin@test.com").Value;
        user.SetOfficialBadge(badge1);

        // Act
        var result = user.SetOfficialBadge(badge2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("User.BadgeAlreadyAssigned");
    }

    [Fact]
    public void RemoveOfficialBadge_ShouldSucceed()
    {
        // Arrange
        var user = CreateTestUser();
        var badge = OfficialBadge.Create(BadgeType.Department, "CS Department", "admin@test.com").Value;
        user.SetOfficialBadge(badge);

        // Act
        var result = user.RemoveOfficialBadge();

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.Badge.Should().BeNull();
    }

    [Fact]
    public void AddRefreshToken_ShouldAddToken()
    {
        // Arrange
        var user = CreateTestUser();
        const string token = "refresh-token";
        var expires = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = user.AddRefreshToken(token, expires);

        // Assert
        user.RefreshTokens.Should().HaveCount(1);
        user.RefreshTokens.First().Should().Be(refreshToken);
        refreshToken.Token.Should().Be(token);
        refreshToken.ExpiresAt.Should().Be(expires);
        refreshToken.UserId.Should().Be(user.Id);
    }

    [Fact]
    public void RevokeRefreshToken_WithExistingToken_ShouldRevokeToken()
    {
        // Arrange
        var user = CreateTestUser();
        const string token = "refresh-token";
        var expires = DateTime.UtcNow.AddDays(7);
        user.AddRefreshToken(token, expires);

        // Act
        user.RevokeRefreshToken(token, "new-token");

        // Assert
        var refreshToken = user.RefreshTokens.First();
        refreshToken.IsActive.Should().BeFalse();
        refreshToken.RevokedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        refreshToken.ReplacedByToken.Should().Be("new-token");
    }

    [Fact]
    public void RevokeAllRefreshTokens_ShouldRevokeAllTokens()
    {
        // Arrange
        var user = CreateTestUser();
        user.AddRefreshToken("token1", DateTime.UtcNow.AddDays(7));
        user.AddRefreshToken("token2", DateTime.UtcNow.AddDays(7));

        // Act
        user.RevokeAllRefreshTokens();

        // Assert
        user.RefreshTokens.Should().AllSatisfy(token => token.IsActive.Should().BeFalse());
    }

    [Theory]
    [InlineData(UserStatus.Active, true)]
    [InlineData(UserStatus.Inactive, false)]
    [InlineData(UserStatus.Suspended, false)]
    [InlineData(UserStatus.Banned, false)]
    public void CanLogin_ShouldReturnCorrectValue(UserStatus status, bool expectedResult)
    {
        // Arrange
        var user = CreateTestUser();
        user.ChangeStatus(status);

        // Act
        var result = user.CanLogin();

        // Assert
        result.Should().Be(expectedResult);
    }

    private static User CreateTestUser()
    {
        var email = Email.Create("test@example.com").Value;
        var profile = UserProfile.Create("John", "Doe").Value;
        return User.Create(email, "hashedPassword", profile).Value;
    }
}