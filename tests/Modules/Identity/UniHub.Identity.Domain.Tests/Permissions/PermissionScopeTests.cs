using UniHub.Identity.Domain.Permissions;
using FluentAssertions;

namespace UniHub.Identity.Domain.Tests.Permissions;

public class PermissionScopeTests
{
    [Fact]
    public void Create_WithGlobalType_ShouldSucceed()
    {
        // Act
        var result = PermissionScope.Create(PermissionScopeType.Global);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(PermissionScopeType.Global);
        result.Value.Value.Should().BeNull();
        result.Value.IsGlobal.Should().BeTrue();
    }

    [Fact]
    public void Create_WithGlobalTypeAndValue_ShouldFail()
    {
        // Act
        var result = PermissionScope.Create(PermissionScopeType.Global, "someValue");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PermissionScope.ValueNotAllowed");
    }

    [Fact]
    public void Create_WithModuleTypeAndValue_ShouldSucceed()
    {
        // Act
        var result = PermissionScope.Create(PermissionScopeType.Module, "forum");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(PermissionScopeType.Module);
        result.Value.Value.Should().Be("forum");
        result.Value.IsGlobal.Should().BeFalse();
    }

    [Fact]
    public void Create_WithModuleTypeAndEmptyValue_ShouldFail()
    {
        // Act
        var result = PermissionScope.Create(PermissionScopeType.Module, "");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PermissionScope.ValueRequired");
    }

    [Fact]
    public void Create_WithTooLongValue_ShouldFail()
    {
        // Arrange
        var longValue = new string('a', 101);

        // Act
        var result = PermissionScope.Create(PermissionScopeType.Course, longValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PermissionScope.ValueTooLong");
    }

    [Fact]
    public void Global_ShouldCreateGlobalScope()
    {
        // Act
        var scope = PermissionScope.Global();

        // Assert
        scope.Type.Should().Be(PermissionScopeType.Global);
        scope.IsGlobal.Should().BeTrue();
        scope.Value.Should().BeNull();
    }

    [Fact]
    public void Module_ShouldCreateModuleScope()
    {
        // Act
        var result = PermissionScope.Module("learning");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(PermissionScopeType.Module);
        result.Value.Value.Should().Be("learning");
    }

    [Fact]
    public void Course_ShouldCreateCourseScope()
    {
        // Act
        var result = PermissionScope.Course("CS101");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(PermissionScopeType.Course);
        result.Value.Value.Should().Be("CS101");
    }

    [Fact]
    public void MatchesScope_WithGlobalScope_ShouldAlwaysMatch()
    {
        // Arrange
        var globalScope = PermissionScope.Global();
        var moduleScope = PermissionScope.Module("forum").Value;

        // Act & Assert
        globalScope.MatchesScope(moduleScope).Should().BeTrue();
        moduleScope.MatchesScope(globalScope).Should().BeTrue();
    }

    [Fact]
    public void MatchesScope_WithSameTypeAndValue_ShouldMatch()
    {
        // Arrange
        var scope1 = PermissionScope.Module("forum").Value;
        var scope2 = PermissionScope.Module("forum").Value;

        // Act & Assert
        scope1.MatchesScope(scope2).Should().BeTrue();
    }

    [Fact]
    public void MatchesScope_WithDifferentValues_ShouldNotMatch()
    {
        // Arrange
        var scope1 = PermissionScope.Module("forum").Value;
        var scope2 = PermissionScope.Module("learning").Value;

        // Act & Assert
        scope1.MatchesScope(scope2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var globalScope = PermissionScope.Global();
        var moduleScope = PermissionScope.Module("forum").Value;

        // Act & Assert
        globalScope.ToString().Should().Be("Global");
        moduleScope.ToString().Should().Be("Module:forum");
    }
}