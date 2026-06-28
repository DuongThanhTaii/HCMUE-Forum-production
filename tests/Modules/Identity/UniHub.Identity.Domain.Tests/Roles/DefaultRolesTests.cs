using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Roles;
using FluentAssertions;

namespace UniHub.Identity.Domain.Tests.Roles;

public class DefaultRolesTests
{
    [Fact]
    public void CreateDefaultRoles_ShouldCreateAllRoles()
    {
        // Act
        var results = DefaultRoles.CreateDefaultRoles().ToList();

        // Assert
        results.Should().HaveCount(6);
        results.Should().OnlyContain(r => r.IsSuccess);

        var roles = results.Select(r => r.Value).ToList();
        roles.Should().Contain(r => r.Name == DefaultRoles.Names.SuperAdmin);
        roles.Should().Contain(r => r.Name == DefaultRoles.Names.Admin);
        roles.Should().Contain(r => r.Name == DefaultRoles.Names.Moderator);
        roles.Should().Contain(r => r.Name == DefaultRoles.Names.Teacher);
        roles.Should().Contain(r => r.Name == DefaultRoles.Names.Student);
        roles.Should().Contain(r => r.Name == DefaultRoles.Names.Guest);
    }

    [Fact]
    public void CreateSuperAdminRole_ShouldCreateSystemRole()
    {
        // Act
        var result = DefaultRoles.CreateSuperAdminRole();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(DefaultRoles.Names.SuperAdmin);
        result.Value.IsSystemRole.Should().BeTrue();
        result.Value.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void CreateStudentRole_ShouldBeDefaultRole()
    {
        // Act
        var result = DefaultRoles.CreateStudentRole();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(DefaultRoles.Names.Student);
        result.Value.IsSystemRole.Should().BeTrue();
        result.Value.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void GetSuperAdminPermissions_ShouldReturnAllPermissions()
    {
        // Act
        var permissions = DefaultRoles.GetSuperAdminPermissions().ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().Contain(PermissionCodes.Identity.UserManage);
        permissions.Should().Contain(PermissionCodes.Forum.PostCreate);
        permissions.Should().Contain(PermissionCodes.Learning.CourseManage);
        
        // Should contain all permissions defined in PermissionCodes
        var allPermissions = PermissionCodes.GetAllPermissions().ToList();
        permissions.Should().BeEquivalentTo(allPermissions);
    }

    [Fact]
    public void GetAdminPermissions_ShouldReturnManagementPermissions()
    {
        // Act
        var permissions = DefaultRoles.GetAdminPermissions().ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().Contain(PermissionCodes.Identity.UserManage);
        permissions.Should().Contain(PermissionCodes.Forum.PostModerate);
        permissions.Should().Contain(PermissionCodes.Learning.CourseManage);
        permissions.Should().NotContain(PermissionCodes.Identity.UserBan); // Super admin only
    }

    [Fact]
    public void GetModeratorPermissions_ShouldReturnModerationPermissions()
    {
        // Act
        var permissions = DefaultRoles.GetModeratorPermissions().ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().Contain(PermissionCodes.Forum.PostModerate);
        permissions.Should().Contain(PermissionCodes.Chat.MessageModerate);
        permissions.Should().Contain(PermissionCodes.Learning.DocumentApprove);
        permissions.Should().NotContain(PermissionCodes.Identity.UserManage); // Admin only
    }

    [Fact]
    public void GetTeacherPermissions_ShouldReturnTeachingPermissions()
    {
        // Act
        var permissions = DefaultRoles.GetTeacherPermissions().ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().Contain(PermissionCodes.Learning.CourseCreate);
        permissions.Should().Contain(PermissionCodes.Learning.MaterialUpload);
        permissions.Should().Contain(PermissionCodes.Forum.PostCreate);
        permissions.Should().NotContain(PermissionCodes.Forum.PostModerate); // Moderator/Admin only
    }

    [Fact]
    public void GetStudentPermissions_ShouldReturnBasicPermissions()
    {
        // Act
        var permissions = DefaultRoles.GetStudentPermissions().ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().Contain(PermissionCodes.Forum.PostCreate);
        permissions.Should().Contain(PermissionCodes.Chat.MessageSend);
        permissions.Should().Contain(PermissionCodes.Learning.CourseEnroll);
        permissions.Should().Contain(PermissionCodes.AI.ChatAccess);
        permissions.Should().NotContain(PermissionCodes.Learning.CourseCreate); // Teacher only
    }

    [Fact]
    public void GetGuestPermissions_ShouldReturnMinimalPermissions()
    {
        // Act
        var permissions = DefaultRoles.GetGuestPermissions().ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().Contain(PermissionCodes.AI.RecommendationView);
        permissions.Should().NotContain(PermissionCodes.Forum.PostCreate); // Requires login
        permissions.Should().NotContain(PermissionCodes.Chat.MessageSend); // Requires login
    }

    [Fact]
    public void AllDefaultRoleNames_ShouldBeUnique()
    {
        // Arrange
        var names = new[]
        {
            DefaultRoles.Names.SuperAdmin,
            DefaultRoles.Names.Admin,
            DefaultRoles.Names.Moderator,
            DefaultRoles.Names.Teacher,
            DefaultRoles.Names.Student,
            DefaultRoles.Names.Guest
        };

        // Act & Assert
        names.Should().OnlyHaveUniqueItems();
        names.Should().OnlyContain(name => !string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void AllDefaultRoleDescriptions_ShouldNotBeEmpty()
    {
        // Arrange
        var descriptions = new[]
        {
            DefaultRoles.Descriptions.SuperAdmin,
            DefaultRoles.Descriptions.Admin,
            DefaultRoles.Descriptions.Moderator,
            DefaultRoles.Descriptions.Teacher,
            DefaultRoles.Descriptions.Student,
            DefaultRoles.Descriptions.Guest
        };

        // Act & Assert
        descriptions.Should().OnlyContain(desc => !string.IsNullOrWhiteSpace(desc));
    }
}