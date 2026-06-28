using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Roles;
using FluentAssertions;

namespace UniHub.Identity.Domain.Tests.Roles;

public class RoleTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        const string name = "Test Role";
        const string description = "Test Description";

        // Act
        var result = Role.Create(name, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.IsDefault.Should().BeFalse();
        result.Value.IsSystemRole.Should().BeFalse();
        result.Value.Permissions.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        // Act
        var result = Role.Create("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.Name.Empty");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldFail()
    {
        // Arrange
        var longName = new string('a', 101);

        // Act
        var result = Role.Create(longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.Name.TooLong");
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldFail()
    {
        // Arrange
        var longDescription = new string('a', 501);

        // Act
        var result = Role.Create("Test Role", longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.Description.TooLong");
    }

    [Fact]
    public void Create_AsDefault_ShouldSetDefaultFlag()
    {
        // Act
        var result = Role.Create("Test Role", isDefault: true);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsDefault.Should().BeTrue();
    }

    [Fact]
    public void CreateSystem_ShouldCreateSystemRole()
    {
        // Act
        var result = Role.CreateSystem("System Role", "System Description");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsSystemRole.Should().BeTrue();
        result.Value.IsDefault.Should().BeFalse();
    }

    [Fact]
    public void UpdateName_WithValidName_ShouldSucceed()
    {
        // Arrange
        var role = Role.Create("Original Name").Value;
        const string newName = "Updated Name";

        // Act
        var result = role.UpdateName(newName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Name.Should().Be(newName);
        role.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateName_OnSystemRole_ShouldFail()
    {
        // Arrange
        var role = Role.CreateSystem("System Role").Value;

        // Act
        var result = role.UpdateName("New Name");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.SystemRole.CannotUpdate");
    }

    [Fact]
    public void UpdateDescription_WithValidDescription_ShouldSucceed()
    {
        // Arrange
        var role = Role.Create("Test Role").Value;
        const string newDescription = "Updated Description";

        // Act
        var result = role.UpdateDescription(newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Description.Should().Be(newDescription);
        role.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UpdateDescription_OnSystemRole_ShouldFail()
    {
        // Arrange
        var role = Role.CreateSystem("System Role").Value;

        // Act
        var result = role.UpdateDescription("New Description");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.SystemRole.CannotUpdate");
    }

    [Fact]
    public void AssignPermission_WithValidData_ShouldSucceed()
    {
        // Arrange
        var role = Role.Create("Test Role").Value;
        var permissionId = PermissionId.CreateUnique();
        var scope = PermissionScope.Global();

        // Act
        var result = role.AssignPermission(permissionId, scope);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Permissions.Should().HaveCount(1);
        role.HasPermission(permissionId, scope).Should().BeTrue();
        role.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AssignPermission_OnSystemRole_ShouldFail()
    {
        // Arrange
        var role = Role.CreateSystem("System Role").Value;
        var permissionId = PermissionId.CreateUnique();
        var scope = PermissionScope.Global();

        // Act
        var result = role.AssignPermission(permissionId, scope);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.SystemRole.CannotModify");
    }

    [Fact]
    public void AssignPermission_WhenAlreadyExists_ShouldFail()
    {
        // Arrange
        var role = Role.Create("Test Role").Value;
        var permissionId = PermissionId.CreateUnique();
        var scope = PermissionScope.Global();
        role.AssignPermission(permissionId, scope);

        // Act
        var result = role.AssignPermission(permissionId, scope);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.Permission.AlreadyAssigned");
    }

    [Fact]
    public void RemovePermission_WithExistingPermission_ShouldSucceed()
    {
        // Arrange
        var role = Role.Create("Test Role").Value;
        var permissionId = PermissionId.CreateUnique();
        var scope = PermissionScope.Global();
        role.AssignPermission(permissionId, scope);

        // Act
        var result = role.RemovePermission(permissionId, scope);

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Permissions.Should().BeEmpty();
        role.HasPermission(permissionId, scope).Should().BeFalse();
        role.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemovePermission_OnSystemRole_ShouldFail()
    {
        // Arrange
        var role = Role.CreateSystem("System Role").Value;
        var permissionId = PermissionId.CreateUnique();
        var scope = PermissionScope.Global();

        // Act
        var result = role.RemovePermission(permissionId, scope);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.SystemRole.CannotModify");
    }

    [Fact]
    public void RemovePermission_WhenNotExists_ShouldFail()
    {
        // Arrange
        var role = Role.Create("Test Role").Value;
        var permissionId = PermissionId.CreateUnique();
        var scope = PermissionScope.Global();

        // Act
        var result = role.RemovePermission(permissionId, scope);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.Permission.NotAssigned");
    }

    [Fact]
    public void SetAsDefault_OnNormalRole_ShouldSucceed()
    {
        // Arrange
        var role = Role.Create("Test Role").Value;

        // Act
        var result = role.SetAsDefault();

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.IsDefault.Should().BeTrue();
        role.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void SetAsDefault_OnSystemRole_ShouldFail()
    {
        // Arrange
        var role = Role.CreateSystem("System Role").Value;

        // Act
        var result = role.SetAsDefault();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Role.SystemRole.CannotSetDefault");
    }

    [Fact]
    public void RemoveAsDefault_ShouldSucceed()
    {
        // Arrange
        var role = Role.Create("Test Role", isDefault: true).Value;

        // Act
        var result = role.RemoveAsDefault();

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.IsDefault.Should().BeFalse();
        role.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemoveAllPermissions_ShouldClearAllPermissions()
    {
        // Arrange
        var role = Role.Create("Test Role").Value;
        var permissionId1 = PermissionId.CreateUnique();
        var permissionId2 = PermissionId.CreateUnique();
        var scope = PermissionScope.Global();
        
        role.AssignPermission(permissionId1, scope);
        role.AssignPermission(permissionId2, scope);

        // Act
        var result = role.RemoveAllPermissions();

        // Assert
        result.IsSuccess.Should().BeTrue();
        role.Permissions.Should().BeEmpty();
        role.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void HasPermission_WithDifferentScopes_ShouldWorkCorrectly()
    {
        // Arrange
        var role = Role.Create("Test Role").Value;
        var permissionId = PermissionId.CreateUnique();
        var globalScope = PermissionScope.Global();
        var moduleScope = PermissionScope.Module("forum").Value;

        role.AssignPermission(permissionId, globalScope);

        // Act & Assert
        role.HasPermission(permissionId, globalScope).Should().BeTrue();
        role.HasPermission(permissionId, moduleScope).Should().BeTrue(); // Global matches all
    }
}