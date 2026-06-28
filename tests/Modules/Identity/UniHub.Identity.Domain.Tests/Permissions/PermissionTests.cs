using UniHub.Identity.Domain.Permissions;
using FluentAssertions;

namespace UniHub.Identity.Domain.Tests.Permissions;

public class PermissionTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        const string code = "forum.post.create";
        const string name = "Create Forum Post";
        const string description = "Allows creating forum posts";

        // Act
        var result = Permission.Create(code, name, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be(code);
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.Module.Should().Be("forum");
        result.Value.Resource.Should().Be("post");
        result.Value.Action.Should().Be("create");
    }

    [Fact]
    public void Create_WithEmptyCode_ShouldFail()
    {
        // Act
        var result = Permission.Create("", "Test Permission");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Permission.Code.Empty");
    }

    [Fact]
    public void Create_WithEmptyName_ShouldFail()
    {
        // Act
        var result = Permission.Create("forum.post.create", "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Permission.Name.Empty");
    }

    [Fact]
    public void Create_WithInvalidCodeFormat_ShouldFail()
    {
        // Act
        var result = Permission.Create("invalidformat", "Test Permission");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Permission.Code.InvalidFormat");
    }

    [Fact]
    public void Create_WithEmptyCodeParts_ShouldFail()
    {
        // Act
        var result = Permission.Create("forum..create", "Test Permission");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Permission.Code.InvalidParts");
    }

    [Fact]
    public void Create_WithTooLongCode_ShouldFail()
    {
        // Arrange
        var longCode = new string('a', 98) + ".b.c"; // 101 characters

        // Act
        var result = Permission.Create(longCode, "Test Permission");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Permission.Code.TooLong");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldFail()
    {
        // Arrange
        var longName = new string('a', 201);

        // Act
        var result = Permission.Create("forum.post.create", longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Permission.Name.TooLong");
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldFail()
    {
        // Arrange
        var longDescription = new string('a', 501);

        // Act
        var result = Permission.Create("forum.post.create", "Test Permission", longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Permission.Description.TooLong");
    }

    [Fact]
    public void Create_ShouldTrimAndLowercaseCode()
    {
        // Act
        var result = Permission.Create("  FORUM.POST.CREATE  ", "Test Permission");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Code.Should().Be("forum.post.create");
        result.Value.Module.Should().Be("forum");
        result.Value.Resource.Should().Be("post");
        result.Value.Action.Should().Be("create");
    }

    [Fact]
    public void Create_ShouldTrimNameAndDescription()
    {
        // Act
        var result = Permission.Create("forum.post.create", "  Test Permission  ", "  Test Description  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test Permission");
        result.Value.Description.Should().Be("Test Description");
    }

    [Fact]
    public void UpdateDescription_WithValidDescription_ShouldSucceed()
    {
        // Arrange
        var permission = Permission.Create("forum.post.create", "Test Permission").Value;
        const string newDescription = "Updated description";

        // Act
        var result = permission.UpdateDescription(newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        permission.Description.Should().Be(newDescription);
    }

    [Fact]
    public void UpdateDescription_WithTooLongDescription_ShouldFail()
    {
        // Arrange
        var permission = Permission.Create("forum.post.create", "Test Permission").Value;
        var longDescription = new string('a', 501);

        // Act
        var result = permission.UpdateDescription(longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Permission.Description.TooLong");
    }

    [Fact]
    public void IsForModule_ShouldReturnCorrectResult()
    {
        // Arrange
        var permission = Permission.Create("forum.post.create", "Test Permission").Value;

        // Act & Assert
        permission.IsForModule("forum").Should().BeTrue();
        permission.IsForModule("FORUM").Should().BeTrue();
        permission.IsForModule("learning").Should().BeFalse();
    }

    [Fact]
    public void IsForResource_ShouldReturnCorrectResult()
    {
        // Arrange
        var permission = Permission.Create("forum.post.create", "Test Permission").Value;

        // Act & Assert
        permission.IsForResource("post").Should().BeTrue();
        permission.IsForResource("POST").Should().BeTrue();
        permission.IsForResource("comment").Should().BeFalse();
    }

    [Fact]
    public void IsForAction_ShouldReturnCorrectResult()
    {
        // Arrange
        var permission = Permission.Create("forum.post.create", "Test Permission").Value;

        // Act & Assert
        permission.IsForAction("create").Should().BeTrue();
        permission.IsForAction("CREATE").Should().BeTrue();
        permission.IsForAction("edit").Should().BeFalse();
    }
}