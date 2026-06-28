using FluentAssertions;
using UniHub.Career.Domain.Recruiters;

namespace UniHub.Career.Domain.Tests.Recruiters;

public class RecruiterPermissionsTests
{
    [Fact]
    public void Create_WithValidPermissions_ShouldCreatePermissions()
    {
        // Act
        var result = RecruiterPermissions.Create(
            canManageJobPostings: true,
            canReviewApplications: true,
            canUpdateApplicationStatus: false,
            canInviteRecruiters: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var permissions = result.Value;
        permissions.CanManageJobPostings.Should().BeTrue();
        permissions.CanReviewApplications.Should().BeTrue();
        permissions.CanUpdateApplicationStatus.Should().BeFalse();
        permissions.CanInviteRecruiters.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNoPermissions_ShouldReturnFailure()
    {
        // Act
        var result = RecruiterPermissions.Create(
            canManageJobPostings: false,
            canReviewApplications: false,
            canUpdateApplicationStatus: false,
            canInviteRecruiters: false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RecruiterErrors.NoPermissionsGranted);
    }

    [Fact]
    public void Create_WithAtLeastOnePermission_ShouldSucceed()
    {
        // Act
        var result = RecruiterPermissions.Create(
            canManageJobPostings: false,
            canReviewApplications: false,
            canUpdateApplicationStatus: false,
            canInviteRecruiters: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CanInviteRecruiters.Should().BeTrue();
    }

    [Fact]
    public void Default_ShouldReturnStandardPermissions()
    {
        // Act
        var permissions = RecruiterPermissions.Default();

        // Assert
        permissions.CanManageJobPostings.Should().BeTrue();
        permissions.CanReviewApplications.Should().BeTrue();
        permissions.CanUpdateApplicationStatus.Should().BeTrue();
        permissions.CanInviteRecruiters.Should().BeFalse();
    }

    [Fact]
    public void Admin_ShouldReturnAllPermissions()
    {
        // Act
        var permissions = RecruiterPermissions.Admin();

        // Assert
        permissions.CanManageJobPostings.Should().BeTrue();
        permissions.CanReviewApplications.Should().BeTrue();
        permissions.CanUpdateApplicationStatus.Should().BeTrue();
        permissions.CanInviteRecruiters.Should().BeTrue();
    }

    [Fact]
    public void EqualPermissions_ShouldBeEqual()
    {
        // Arrange
        var permissions1 = RecruiterPermissions.Default();
        var permissions2 = RecruiterPermissions.Default();

        // Act & Assert
        permissions1.Should().Be(permissions2);
    }

    [Fact]
    public void DifferentPermissions_ShouldNotBeEqual()
    {
        // Arrange
        var permissions1 = RecruiterPermissions.Default();
        var permissions2 = RecruiterPermissions.Admin();

        // Act & Assert
        permissions1.Should().NotBe(permissions2);
    }
}
