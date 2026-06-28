using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Queries.GetUserPermissions;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;
using UniHub.Identity.Infrastructure;

namespace UniHub.Identity.Infrastructure.Tests.Authorization;

public class PermissionCheckerOverrideTests
{
    private static async Task<ServiceProvider> BuildProviderAsync()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:SecretKey"] = "ThisIsATestSecretKeyForPermissionCheckerOverrideTests_1234567890",
            ["Jwt:Issuer"] = "UniHub.Tests",
            ["Jwt:Audience"] = "UniHub.Tests.Client",
            ["Jwt:AccessTokenExpiryMinutes"] = "15",
            ["Jwt:RefreshTokenExpiryDays"] = "7"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddIdentityInfrastructure(configuration);

        var store = new InMemoryPermissionStore();
        store.SeedPermission(Permission.Create(
            "forum.post.create",
            "Create forum post",
            "Seeded permission for permission checker tests").Value);

        services.AddSingleton(store);
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        services.AddSingleton<IRoleRepository, InMemoryRoleRepository>();
        services.AddSingleton<IPermissionRepository, InMemoryPermissionRepository>();
        services.AddSingleton<IUserGroupRepository, InMemoryUserGroupRepository>();
        services.AddSingleton<IUserPermissionOverrideRepository, InMemoryUserPermissionOverrideRepository>();
        services.AddSingleton<IGroupPermissionOverrideRepository, InMemoryGroupPermissionOverrideRepository>();
        services.AddSingleton<IPermissionCache, NoopPermissionCache>();

        return await Task.FromResult(services.BuildServiceProvider());
    }

    private static async Task<User> CreateUserAsync(IUserRepository userRepository, string emailAddress)
    {
        var email = Email.Create(emailAddress).Value;
        var profile = UserProfile.Create("Test", "User").Value;
        var user = User.Create(email, "hashed-password", profile).Value;

        await userRepository.AddAsync(user);
        return user;
    }

    [Fact]
    public async Task HasPermissionAsync_WhenRoleAllowsAndNoOverrides_ShouldReturnTrue()
    {
        using var provider = await BuildProviderAsync();
        using var scope = provider.CreateScope();

        var checker = scope.ServiceProvider.GetRequiredService<IPermissionChecker>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var user = await CreateUserAsync(userRepository, $"role-allow-{Guid.NewGuid():N}@example.com");
        var role = Role.Create($"TestRole-{Guid.NewGuid():N}", "Role for permission checker tests").Value;
        var permission = await permissionRepository.GetByCodeAsync("forum.post.create");

        permission.Should().NotBeNull();

        await roleRepository.AddAsync(role);

        user.AssignRole(role.Id).IsSuccess.Should().BeTrue();
        role.AssignPermission(permission!.Id, PermissionScope.Global()).IsSuccess.Should().BeTrue();

        var result = await checker.HasPermissionAsync(user.Id, "forum.post.create");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserDenyOverrideExists_ShouldReturnFalse()
    {
        using var provider = await BuildProviderAsync();
        using var scope = provider.CreateScope();

        var checker = scope.ServiceProvider.GetRequiredService<IPermissionChecker>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        var userOverrideRepository = scope.ServiceProvider.GetRequiredService<IUserPermissionOverrideRepository>();

        var user = await CreateUserAsync(userRepository, $"user-deny-{Guid.NewGuid():N}@example.com");
        var role = Role.Create($"TestRole-{Guid.NewGuid():N}", "Role for permission checker tests").Value;
        var permission = await permissionRepository.GetByCodeAsync("forum.post.create");

        permission.Should().NotBeNull();

        await roleRepository.AddAsync(role);

        user.AssignRole(role.Id).IsSuccess.Should().BeTrue();
        role.AssignPermission(permission!.Id, PermissionScope.Global()).IsSuccess.Should().BeTrue();

        var denyOverride = UserPermissionOverride.Create(
            user.Id,
            permission.Id,
            PermissionScope.Global(),
            PermissionEffect.Deny,
            "Block this permission").Value;

        await userOverrideRepository.AddAsync(denyOverride);

        var result = await checker.HasPermissionAsync(user.Id, "forum.post.create");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenGroupAllowOverrideExists_ShouldReturnTrue()
    {
        using var provider = await BuildProviderAsync();
        using var scope = provider.CreateScope();

        var checker = scope.ServiceProvider.GetRequiredService<IPermissionChecker>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        var userGroupRepository = scope.ServiceProvider.GetRequiredService<IUserGroupRepository>();
        var groupOverrideRepository = scope.ServiceProvider.GetRequiredService<IGroupPermissionOverrideRepository>();

        var user = await CreateUserAsync(userRepository, $"group-allow-{Guid.NewGuid():N}@example.com");
        var permission = await permissionRepository.GetByCodeAsync("forum.post.create");
        permission.Should().NotBeNull();

        var group = UserGroup.Create($"group-{Guid.NewGuid():N}").Value;
        group.AddMember(user.Id).IsSuccess.Should().BeTrue();
        await userGroupRepository.AddAsync(group);

        var allowOverride = GroupPermissionOverride.Create(
            group.Id,
            permission!.Id,
            PermissionScope.Global(),
            PermissionEffect.Allow,
            "Group can create post").Value;

        await groupOverrideRepository.AddAsync(allowOverride);

        var result = await checker.HasPermissionAsync(user.Id, "forum.post.create");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenUserAllowAndGroupDenyExist_ShouldReturnTrue()
    {
        using var provider = await BuildProviderAsync();
        using var scope = provider.CreateScope();

        var checker = scope.ServiceProvider.GetRequiredService<IPermissionChecker>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        var userGroupRepository = scope.ServiceProvider.GetRequiredService<IUserGroupRepository>();
        var userOverrideRepository = scope.ServiceProvider.GetRequiredService<IUserPermissionOverrideRepository>();
        var groupOverrideRepository = scope.ServiceProvider.GetRequiredService<IGroupPermissionOverrideRepository>();

        var user = await CreateUserAsync(userRepository, $"user-allow-group-deny-{Guid.NewGuid():N}@example.com");
        var permission = await permissionRepository.GetByCodeAsync("forum.post.create");
        permission.Should().NotBeNull();

        var group = UserGroup.Create($"group-{Guid.NewGuid():N}").Value;
        group.AddMember(user.Id).IsSuccess.Should().BeTrue();
        await userGroupRepository.AddAsync(group);

        var groupDeny = GroupPermissionOverride.Create(
            group.Id,
            permission!.Id,
            PermissionScope.Global(),
            PermissionEffect.Deny,
            "Group denied").Value;

        await groupOverrideRepository.AddAsync(groupDeny);

        var userAllow = UserPermissionOverride.Create(
            user.Id,
            permission.Id,
            PermissionScope.Global(),
            PermissionEffect.Allow,
            "User explicitly allowed").Value;

        await userOverrideRepository.AddAsync(userAllow);

        var result = await checker.HasPermissionAsync(user.Id, "forum.post.create");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WhenRoleDoesNotGrantButUserAllowOverrideExists_ShouldReturnTrue()
    {
        using var provider = await BuildProviderAsync();
        using var scope = provider.CreateScope();

        var checker = scope.ServiceProvider.GetRequiredService<IPermissionChecker>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var permissionRepository = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();
        var userOverrideRepository = scope.ServiceProvider.GetRequiredService<IUserPermissionOverrideRepository>();

        var user = await CreateUserAsync(userRepository, $"role-deny-user-allow-{Guid.NewGuid():N}@example.com");
        var role = Role.Create($"NoGrantRole-{Guid.NewGuid():N}", "Role without target permission").Value;
        var deniedPermission = Permission.Create(
            "forum.post.delete",
            "Delete forum post",
            "Synthetic permission to keep role missing target grant").Value;

        var targetPermission = await permissionRepository.GetByCodeAsync("forum.post.create");
        targetPermission.Should().NotBeNull();

        await roleRepository.AddAsync(role);

        user.AssignRole(role.Id).IsSuccess.Should().BeTrue();
        role.AssignPermission(deniedPermission.Id, PermissionScope.Global()).IsSuccess.Should().BeTrue();

        var userAllow = UserPermissionOverride.Create(
            user.Id,
            targetPermission!.Id,
            PermissionScope.Global(),
            PermissionEffect.Allow,
            "User explicitly allowed").Value;

        await userOverrideRepository.AddAsync(userAllow);

        var result = await checker.HasPermissionAsync(user.Id, "forum.post.create");

        result.Should().BeTrue();
    }
}

internal sealed class InMemoryPermissionStore
{
    public Dictionary<Guid, User> Users { get; } = new();
    public Dictionary<Guid, Role> Roles { get; } = new();
    public Dictionary<Guid, Permission> Permissions { get; } = new();
    public Dictionary<Guid, UserGroup> Groups { get; } = new();
    public List<UserPermissionOverride> UserOverrides { get; } = new();
    public List<GroupPermissionOverride> GroupOverrides { get; } = new();

    public void SeedPermission(Permission permission)
    {
        Permissions[permission.Id.Value] = permission;
    }
}

internal sealed class InMemoryUserRepository(InMemoryPermissionStore store) : IUserRepository
{
    public Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyList<User>)store.Users.Values.ToList());

    public Task<User?> GetByIdAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        store.Users.TryGetValue(userId.Value, out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var user = store.Users.Values.FirstOrDefault(item => item.Email == email);
        return Task.FromResult(user);
    }

    public Task<bool> IsEmailUniqueAsync(Email email, CancellationToken cancellationToken = default)
        => Task.FromResult(store.Users.Values.All(item => item.Email != email));

    public Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        store.Users[user.Id.Value] = user;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        store.Users[user.Id.Value] = user;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        store.Users.Remove(user.Id.Value);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<User>> SearchAsync(string search, int take, CancellationToken cancellationToken = default)
    {
        var result = store.Users.Values
            .Where(u => u.Email.Value.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                        u.Profile.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
            .Take(take)
            .ToList();
        return Task.FromResult((IReadOnlyList<User>)result);
    }
}

internal sealed class InMemoryRoleRepository(InMemoryPermissionStore store) : IRoleRepository
{
    public Task<Role?> GetByIdAsync(RoleId roleId, CancellationToken cancellationToken = default)
    {
        store.Roles.TryGetValue(roleId.Value, out var role);
        return Task.FromResult(role);
    }

    public Task<List<Role>> GetByIdsAsync(IEnumerable<RoleId> roleIds, CancellationToken cancellationToken = default)
    {
        var list = roleIds
            .Select(id => store.Roles.TryGetValue(id.Value, out var r) ? r : null)
            .Where(r => r is not null)
            .Cast<Role>()
            .ToList();
        return Task.FromResult(list);
    }

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var role = store.Roles.Values.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(role);
    }

    public Task<List<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(store.Roles.Values.ToList());

    public Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        store.Roles[role.Id.Value] = role;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        store.Roles[role.Id.Value] = role;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        store.Roles.Remove(role.Id.Value);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryPermissionRepository(InMemoryPermissionStore store) : IPermissionRepository
{
    public Task<Permission?> GetByIdAsync(PermissionId id, CancellationToken cancellationToken = default)
    {
        store.Permissions.TryGetValue(id.Value, out var permission);
        return Task.FromResult(permission);
    }

    public Task<Permission?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var permission = store.Permissions.Values.FirstOrDefault(item =>
            string.Equals(item.Code, code, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(permission);
    }

    public Task<IReadOnlyList<Permission>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult((IReadOnlyList<Permission>)store.Permissions.Values.ToList());
}

internal sealed class InMemoryUserGroupRepository(InMemoryPermissionStore store) : IUserGroupRepository
{
    public Task<UserGroup?> GetByIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        store.Groups.TryGetValue(groupId, out var group);
        return Task.FromResult(group);
    }

    public Task<UserGroup?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var group = store.Groups.Values.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(group);
    }

    public Task<List<UserGroup>> GetByMemberAsync(UserId userId, CancellationToken cancellationToken = default)
    {
        var groups = store.Groups.Values
            .Where(group => group.Members.Any(member => member.UserId == userId))
            .ToList();

        return Task.FromResult(groups);
    }

    public Task<List<UserGroup>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(store.Groups.Values.ToList());

    public Task AddAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        store.Groups[group.Id] = group;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        store.Groups[group.Id] = group;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(UserGroup group, CancellationToken cancellationToken = default)
    {
        store.Groups.Remove(group.Id);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryUserPermissionOverrideRepository(InMemoryPermissionStore store) : IUserPermissionOverrideRepository
{
    public Task<List<UserPermissionOverride>> GetEffectiveByUserAsync(UserId userId, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        var effective = store.UserOverrides
            .Where(item => item.UserId == userId && item.IsEffectiveAt(asOfUtc))
            .ToList();

        return Task.FromResult(effective);
    }

    public Task<UserPermissionOverride?> GetByKeyAsync(UserId userId, PermissionId permissionId, PermissionScope scope, CancellationToken cancellationToken = default)
    {
        var item = store.UserOverrides.FirstOrDefault(overrideItem =>
            overrideItem.UserId == userId &&
            overrideItem.PermissionId == permissionId &&
            overrideItem.Scope.Equals(scope));

        return Task.FromResult(item);
    }

    public Task AddAsync(UserPermissionOverride overrideItem, CancellationToken cancellationToken = default)
    {
        store.UserOverrides.Add(overrideItem);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserPermissionOverride overrideItem, CancellationToken cancellationToken = default)
    {
        var existingIndex = store.UserOverrides.FindIndex(item => item.Id == overrideItem.Id);
        if (existingIndex >= 0)
        {
            store.UserOverrides[existingIndex] = overrideItem;
        }

        return Task.CompletedTask;
    }
}

internal sealed class InMemoryGroupPermissionOverrideRepository(InMemoryPermissionStore store) : IGroupPermissionOverrideRepository
{
    public Task<List<GroupPermissionOverride>> GetEffectiveByGroupAsync(Guid groupId, DateTime asOfUtc, CancellationToken cancellationToken = default)
    {
        var effective = store.GroupOverrides
            .Where(item => item.GroupId == groupId && item.IsEffectiveAt(asOfUtc))
            .ToList();

        return Task.FromResult(effective);
    }

    public Task<GroupPermissionOverride?> GetByKeyAsync(Guid groupId, PermissionId permissionId, PermissionScope scope, CancellationToken cancellationToken = default)
    {
        var item = store.GroupOverrides.FirstOrDefault(overrideItem =>
            overrideItem.GroupId == groupId &&
            overrideItem.PermissionId == permissionId &&
            overrideItem.Scope.Equals(scope));

        return Task.FromResult(item);
    }

    public Task AddAsync(GroupPermissionOverride overrideItem, CancellationToken cancellationToken = default)
    {
        store.GroupOverrides.Add(overrideItem);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(GroupPermissionOverride overrideItem, CancellationToken cancellationToken = default)
    {
        var existingIndex = store.GroupOverrides.FindIndex(item => item.Id == overrideItem.Id);
        if (existingIndex >= 0)
        {
            store.GroupOverrides[existingIndex] = overrideItem;
        }

        return Task.CompletedTask;
    }
}

internal sealed class NoopPermissionCache : IPermissionCache
{
    public Task<UserPermissionsResponse?> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.FromResult<UserPermissionsResponse?>(null);

    public Task SetUserPermissionsAsync(Guid userId, UserPermissionsResponse permissions, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InvalidateUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task InvalidateAllAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
