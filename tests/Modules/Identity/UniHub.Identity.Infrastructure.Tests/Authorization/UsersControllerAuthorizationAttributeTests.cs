using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using UniHub.Identity.Presentation.Controllers;

namespace UniHub.Identity.Infrastructure.Tests.Authorization;

public sealed class UsersControllerAuthorizationAttributeTests
{
    [Theory]
    [InlineData(nameof(UsersController.AssignRole))]
    [InlineData(nameof(UsersController.RemoveRole))]
    [InlineData(nameof(UsersController.AssignBadge))]
    [InlineData(nameof(UsersController.RemoveBadge))]
    public void AdminEndpoints_ShouldRequireAdminRole(string methodName)
    {
        var method = typeof(UsersController)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(item => item.Name == methodName);

        var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.Should().NotBeNull();
        authorizeAttribute!.Roles.Should().Be("Admin");
    }
}
