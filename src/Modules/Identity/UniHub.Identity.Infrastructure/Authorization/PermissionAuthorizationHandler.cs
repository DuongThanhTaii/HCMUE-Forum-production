using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Infrastructure.Authorization;

/// <summary>
/// Handles <see cref="PermissionRequirement"/> by delegating to <see cref="IPermissionChecker"/>.
/// Registered as Singleton in DI — uses IServiceScopeFactory to resolve Scoped dependencies safely.
/// </summary>
public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PermissionAuthorizationHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userGuid))
        {
            // Not authenticated — handled by [Authorize] RequireAuthenticatedUser, not 403
            context.Fail();
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var checker = scope.ServiceProvider.GetRequiredService<IPermissionChecker>();

        var hasPermission = await checker.HasPermissionAsync(
            new UserId(userGuid),
            requirement.PermissionCode);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        // Implicit fail → 403 Forbidden
    }
}
