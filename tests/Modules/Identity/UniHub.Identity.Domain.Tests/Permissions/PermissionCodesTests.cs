using UniHub.Identity.Domain.Permissions;
using FluentAssertions;

namespace UniHub.Identity.Domain.Tests.Permissions;

public class PermissionCodesTests
{
    [Fact]
    public void GetAllPermissions_ShouldReturnAllDefinedPermissions()
    {
        // Act
        var permissions = PermissionCodes.GetAllPermissions().ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().Contain(PermissionCodes.Forum.PostCreate);
        permissions.Should().Contain(PermissionCodes.Learning.DocumentUpload);
        permissions.Should().Contain(PermissionCodes.Identity.UserManage);
        permissions.Should().Contain(PermissionCodes.Career.JobCreate);
        permissions.Should().Contain(PermissionCodes.Chat.MessageSend);
        permissions.Should().Contain(PermissionCodes.Notification.NotificationSend);
        permissions.Should().Contain(PermissionCodes.AI.ChatAccess);
    }

    [Fact]
    public void GetModulePermissions_WithForumModule_ShouldReturnOnlyForumPermissions()
    {
        // Act
        var permissions = PermissionCodes.GetModulePermissions("forum").ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().OnlyContain(p => p.StartsWith("forum."));
        permissions.Should().Contain(PermissionCodes.Forum.PostCreate);
        permissions.Should().Contain(PermissionCodes.Forum.CommentCreate);
        permissions.Should().NotContain(PermissionCodes.Learning.DocumentUpload);
    }

    [Fact]
    public void GetModulePermissions_WithLearningModule_ShouldReturnOnlyLearningPermissions()
    {
        // Act
        var permissions = PermissionCodes.GetModulePermissions("learning").ToList();

        // Assert
        permissions.Should().NotBeEmpty();
        permissions.Should().OnlyContain(p => p.StartsWith("learning."));
        permissions.Should().Contain(PermissionCodes.Learning.DocumentUpload);
        permissions.Should().Contain(PermissionCodes.Learning.CourseManage);
        permissions.Should().NotContain(PermissionCodes.Forum.PostCreate);
    }

    [Fact]
    public void GetModulePermissions_WithNonExistentModule_ShouldReturnEmpty()
    {
        // Act
        var permissions = PermissionCodes.GetModulePermissions("nonexistent").ToList();

        // Assert
        permissions.Should().BeEmpty();
    }

    [Theory]
    [InlineData("forum.post.create")]
    [InlineData("learning.document.upload")]
    [InlineData("identity.user.manage")]
    [InlineData("career.job.create")]
    [InlineData("chat.message.send")]
    [InlineData("notification.send")]
    [InlineData("ai.chat.access")]
    public void PermissionCodes_ShouldFollowCorrectFormat(string permissionCode)
    {
        // Act
        var parts = permissionCode.Split('.');

        // Assert
        parts.Should().HaveCountGreaterOrEqualTo(2);
        parts[0].Should().NotBeNullOrWhiteSpace(); // module
        
        if (parts.Length >= 3)
        {
            parts[1].Should().NotBeNullOrWhiteSpace(); // resource
            parts[2].Should().NotBeNullOrWhiteSpace(); // action
        }
    }

    [Fact]
    public void ForumPermissions_ShouldHaveExpectedCodes()
    {
        // Assert
        PermissionCodes.Forum.PostCreate.Should().Be("forum.post.create");
        PermissionCodes.Forum.PostEdit.Should().Be("forum.post.edit");
        PermissionCodes.Forum.PostDelete.Should().Be("forum.post.delete");
        PermissionCodes.Forum.PostModerate.Should().Be("forum.post.moderate");
        PermissionCodes.Forum.CommentCreate.Should().Be("forum.comment.create");
        PermissionCodes.Forum.CommentDelete.Should().Be("forum.comment.delete");
    }

    [Fact]
    public void LearningPermissions_ShouldHaveExpectedCodes()
    {
        // Assert
        PermissionCodes.Learning.DocumentUpload.Should().Be("learning.document.upload");
        PermissionCodes.Learning.DocumentApprove.Should().Be("learning.document.approve");
        PermissionCodes.Learning.CourseManage.Should().Be("learning.course.manage");
        PermissionCodes.Learning.CourseCreate.Should().Be("learning.course.create");
    }

    [Fact]
    public void IdentityPermissions_ShouldHaveExpectedCodes()
    {
        // Assert
        PermissionCodes.Identity.UserManage.Should().Be("identity.user.manage");
        PermissionCodes.Identity.RoleManage.Should().Be("identity.role.manage");
        PermissionCodes.Identity.PermissionAssign.Should().Be("identity.permission.assign");
        PermissionCodes.Identity.BadgeManage.Should().Be("identity.badge.manage");
    }
}