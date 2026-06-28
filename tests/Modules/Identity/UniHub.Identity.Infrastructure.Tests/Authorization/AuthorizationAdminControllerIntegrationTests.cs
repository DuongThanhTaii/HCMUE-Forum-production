using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using UniHub.Contracts;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Commands.Authorization.UpsertUserPermissionOverride;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.Identity.Infrastructure.Persistence.Repositories;
using UniHub.Identity.Presentation.Controllers;
using UniHub.Identity.Presentation.DTOs.Authorization;

namespace UniHub.Identity.Infrastructure.Tests.Authorization;

public sealed class AuthorizationAdminControllerIntegrationTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();

        var store = new InMemoryPermissionStore();
        store.SeedPermission(Permission.Create(
            "forum.post.create",
            "Create forum post",
            "Seeded permission for admin controller tests").Value);

        services.AddSingleton(store);
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IRoleRepository, InMemoryRoleRepository>();
        services.AddSingleton<IPermissionRepository, InMemoryPermissionRepository>();
        services.AddSingleton<IUserGroupRepository, InMemoryUserGroupRepository>();
        services.AddSingleton<IUserPermissionOverrideRepository, InMemoryUserPermissionOverrideRepository>();
        services.AddSingleton<IGroupPermissionOverrideRepository, InMemoryGroupPermissionOverrideRepository>();
        services.AddSingleton<IEndpointToggleRepository, EndpointToggleRepository>();
        services.AddSingleton<IAuthorizationAuditLogRepository, AuthorizationAuditLogRepository>();
        services.AddSingleton<IPermissionCache, NoopPermissionCache>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<UpsertUserPermissionOverrideCommand>();
        });

        return services.BuildServiceProvider();
    }

    private static async Task<User> CreateUserAsync(IUserRepository userRepository, string emailAddress)
    {
        var email = Email.Create(emailAddress).Value;
        var profile = UserProfile.Create("Admin", "Tester").Value;
        var user = User.Create(email, "hashed-password", profile).Value;

        await userRepository.AddAsync(user);
        return user;
    }

    private static AuthorizationAdminController CreateController(IServiceProvider serviceProvider)
    {
        var sender = serviceProvider.GetRequiredService<ISender>();
        var userGroupRepository = serviceProvider.GetRequiredService<IUserGroupRepository>();
        var endpointToggleRepository = serviceProvider.GetRequiredService<IEndpointToggleRepository>();
        return new AuthorizationAdminController(
            sender,
            userGroupRepository,
            endpointToggleRepository,
            Array.Empty<EndpointDataSource>());
    }

    [Fact]
    public async Task UserOverrideEndpoints_ShouldUpsertGetAndRevokeSuccessfully()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var user = await CreateUserAsync(userRepository, $"admin-user-{Guid.NewGuid():N}@example.com");
        var permission = await permissionRepository.GetByCodeAsync("forum.post.create");
        permission.Should().NotBeNull();

        var controller = CreateController(scope.ServiceProvider);

        var upsertRequest = new UpsertPermissionOverrideRequest(
            permission!.Id.Value,
            "Global",
            null,
            "Allow",
            "Integration test",
            null);

        var upsertResult = await controller.UpsertUserOverride(user.Id.Value, upsertRequest, CancellationToken.None);
        upsertResult.Should().BeOfType<OkObjectResult>();

        var getResult = await controller.GetUserOverrides(user.Id.Value, CancellationToken.None);
        var getOk = getResult.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = getOk.Value.Should().BeOfType<ApiResponse<IReadOnlyList<PermissionOverrideResponse>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        var response = envelope.Data!;

        response.Should().ContainSingle();
        response[0].PermissionId.Should().Be(permission.Id.Value);
        response[0].Effect.Should().Be("Allow");

        var revokeResult = await controller.RevokeUserOverride(
            user.Id.Value,
            permission.Id.Value,
            "Global",
            null,
            CancellationToken.None);

        revokeResult.Should().BeOfType<OkObjectResult>();

        var getAfterRevoke = await controller.GetUserOverrides(user.Id.Value, CancellationToken.None);
        var getAfterRevokeOk = getAfterRevoke.Should().BeOfType<OkObjectResult>().Subject;
        var revokeEnvelope = getAfterRevokeOk.Value.Should().BeOfType<ApiResponse<IReadOnlyList<PermissionOverrideResponse>>>().Subject;
        revokeEnvelope.Success.Should().BeTrue();
        revokeEnvelope.Data.Should().NotBeNull();
        var responseAfterRevoke = revokeEnvelope.Data!;

        responseAfterRevoke.Should().BeEmpty();
    }

    [Fact]
    public async Task GroupOverrideEndpoints_ShouldUpsertGetAndRevokeSuccessfully()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        var userGroupRepository = scope.ServiceProvider.GetRequiredService<IUserGroupRepository>();

        var group = UserGroup.Create($"admin-group-{Guid.NewGuid():N}").Value;
        await userGroupRepository.AddAsync(group);

        var permission = await permissionRepository.GetByCodeAsync("forum.post.create");
        permission.Should().NotBeNull();

        var controller = CreateController(scope.ServiceProvider);

        var upsertRequest = new UpsertPermissionOverrideRequest(
            permission!.Id.Value,
            "Global",
            null,
            "Deny",
            "Integration group test",
            null);

        var upsertResult = await controller.UpsertGroupOverride(group.Id, upsertRequest, CancellationToken.None);
        upsertResult.Should().BeOfType<OkObjectResult>();

        var getResult = await controller.GetGroupOverrides(group.Id, CancellationToken.None);
        var getOk = getResult.Should().BeOfType<OkObjectResult>().Subject;
        var envelope = getOk.Value.Should().BeOfType<ApiResponse<IReadOnlyList<PermissionOverrideResponse>>>().Subject;
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        var response = envelope.Data!;

        response.Should().ContainSingle();
        response[0].PermissionId.Should().Be(permission.Id.Value);
        response[0].Effect.Should().Be("Deny");

        var revokeResult = await controller.RevokeGroupOverride(
            group.Id,
            permission.Id.Value,
            "Global",
            null,
            CancellationToken.None);

        revokeResult.Should().BeOfType<OkObjectResult>();

        var getAfterRevoke = await controller.GetGroupOverrides(group.Id, CancellationToken.None);
        var getAfterRevokeOk = getAfterRevoke.Should().BeOfType<OkObjectResult>().Subject;
        var revokeEnvelope = getAfterRevokeOk.Value.Should().BeOfType<ApiResponse<IReadOnlyList<PermissionOverrideResponse>>>().Subject;
        revokeEnvelope.Success.Should().BeTrue();
        revokeEnvelope.Data.Should().NotBeNull();
        var responseAfterRevoke = revokeEnvelope.Data!;

        responseAfterRevoke.Should().BeEmpty();
    }

    [Fact]
    public async Task EndpointToggleEndpoints_ShouldSetGetAndListSuccessfully()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var controller = CreateController(scope.ServiceProvider);
        const string endpointKey = "Api.Identity.AuthorizationAdmin.SetEndpointToggle";

        var getMissing = await controller.GetEndpointToggleByKey(endpointKey, CancellationToken.None);
        getMissing.Should().BeOfType<NotFoundObjectResult>();

        var disableResult = await controller.SetEndpointToggle(
            endpointKey,
            new SetEndpointToggleRequest(false, "Maintenance window"),
            CancellationToken.None);

        var disableOk = disableResult.Should().BeOfType<OkObjectResult>().Subject;
        var disableEnvelope = disableOk.Value.Should().BeOfType<ApiResponse<EndpointToggleResponse>>().Subject;
        disableEnvelope.Success.Should().BeTrue();
        disableEnvelope.Data.Should().NotBeNull();
        var disablePayload = disableEnvelope.Data!;
        disablePayload.IsEnabled.Should().BeFalse();
        disablePayload.Reason.Should().Be("Maintenance window");

        var getSingle = await controller.GetEndpointToggleByKey(endpointKey, CancellationToken.None);
        var getSingleOk = getSingle.Should().BeOfType<OkObjectResult>().Subject;
        var singleEnvelope = getSingleOk.Value.Should().BeOfType<ApiResponse<EndpointToggleResponse>>().Subject;
        singleEnvelope.Success.Should().BeTrue();
        singleEnvelope.Data.Should().NotBeNull();
        var getSinglePayload = singleEnvelope.Data!;
        getSinglePayload.IsEnabled.Should().BeFalse();

        var listResult = await controller.GetEndpointToggles(CancellationToken.None);
        var listOk = listResult.Should().BeOfType<OkObjectResult>().Subject;
        var listEnvelope = listOk.Value.Should().BeOfType<ApiResponse<IReadOnlyList<EndpointToggleResponse>>>().Subject;
        listEnvelope.Success.Should().BeTrue();
        listEnvelope.Data.Should().NotBeNull();
        var listPayload = listEnvelope.Data!;
        listPayload.Should().Contain(item => item.EndpointKey == endpointKey && !item.IsEnabled);

        var enableResult = await controller.SetEndpointToggle(
            endpointKey,
            new SetEndpointToggleRequest(true, null),
            CancellationToken.None);

        var enableOk = enableResult.Should().BeOfType<OkObjectResult>().Subject;
        var enableEnvelope = enableOk.Value.Should().BeOfType<ApiResponse<EndpointToggleResponse>>().Subject;
        enableEnvelope.Success.Should().BeTrue();
        enableEnvelope.Data.Should().NotBeNull();
        var enablePayload = enableEnvelope.Data!;
        enablePayload.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task AuthorizationAuditLogEndpoint_ShouldReturnToggleAuditLogs()
    {
        using var provider = BuildProvider();
        using var scope = provider.CreateScope();

        var controller = CreateController(scope.ServiceProvider);
        const string endpointKey = "Api.Identity.AuthorizationAdmin.GetUserOverrides";

        await controller.SetEndpointToggle(
            endpointKey,
            new SetEndpointToggleRequest(false, "Emergency disable"),
            CancellationToken.None);

        var auditResult = await controller.GetAuthorizationAuditLogs(
            userId: null,
            endpointKey: endpointKey,
            isSuccess: true,
            fromUtc: null,
            toUtc: null,
            take: 20,
            cancellationToken: CancellationToken.None);

        var auditOk = auditResult.Should().BeOfType<OkObjectResult>().Subject;
        var auditEnvelope = auditOk.Value.Should().BeOfType<ApiResponse<IReadOnlyList<AuthorizationAuditLogResponse>>>().Subject;
        auditEnvelope.Success.Should().BeTrue();
        auditEnvelope.Data.Should().NotBeNull();
        var auditPayload = auditEnvelope.Data!;

        auditPayload.Should().NotBeEmpty();
        auditPayload.Should().Contain(item =>
            item.TargetType == "EndpointToggle"
            && item.TargetKey == endpointKey
            && item.IsSuccess);
    }
}
