using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Commands.Authorization.UpsertUserPermissionOverride;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Application.Tests.Commands.Authorization;

public sealed class UpsertUserPermissionOverrideCommandHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUserPermissionOverrideRepository _overrideRepository;
    private readonly IPermissionCache _permissionCache;
    private readonly UpsertUserPermissionOverrideCommandHandler _handler;

    public UpsertUserPermissionOverrideCommandHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _overrideRepository = Substitute.For<IUserPermissionOverrideRepository>();
        _permissionCache = Substitute.For<IPermissionCache>();

        _handler = new UpsertUserPermissionOverrideCommandHandler(
            _userRepository,
            _permissionRepository,
            _overrideRepository,
            _permissionCache);
    }

    [Fact]
    public async Task Handle_WhenNoExistingOverride_ShouldCreateNewOverride()
    {
        var user = User.Create(
            Email.Create("override-user@example.com").Value,
            "hashed-password",
            UserProfile.Create("Override", "User").Value).Value;

        var permission = Permission.Create("forum.post.create", "Create post").Value;

        var command = new UpsertUserPermissionOverrideCommand(
            user.Id.Value,
            permission.Id.Value,
            "Global",
            null,
            "Allow",
            "Grant explicitly",
            null);

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _permissionRepository.GetByIdAsync(permission.Id, Arg.Any<CancellationToken>()).Returns(permission);
        _overrideRepository.GetByKeyAsync(user.Id, permission.Id, Arg.Any<PermissionScope>(), Arg.Any<CancellationToken>())
            .Returns((UserPermissionOverride?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _overrideRepository.Received(1).AddAsync(Arg.Any<UserPermissionOverride>(), Arg.Any<CancellationToken>());
        await _permissionCache.Received(1).InvalidateUserPermissionsAsync(user.Id.Value, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenActiveOverrideExists_ShouldUpdateExistingOverride()
    {
        var user = User.Create(
            Email.Create("override-update@example.com").Value,
            "hashed-password",
            UserProfile.Create("Update", "User").Value).Value;

        var permission = Permission.Create("forum.post.create", "Create post").Value;
        var existingOverride = UserPermissionOverride.Create(
            user.Id,
            permission.Id,
            PermissionScope.Global(),
            PermissionEffect.Deny,
            "Initial deny").Value;

        var command = new UpsertUserPermissionOverrideCommand(
            user.Id.Value,
            permission.Id.Value,
            "Global",
            null,
            "Allow",
            "Updated allow",
            null);

        _userRepository.GetByIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(user);
        _permissionRepository.GetByIdAsync(permission.Id, Arg.Any<CancellationToken>()).Returns(permission);
        _overrideRepository.GetByKeyAsync(user.Id, permission.Id, Arg.Any<PermissionScope>(), Arg.Any<CancellationToken>())
            .Returns(existingOverride);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingOverride.Effect.Should().Be(PermissionEffect.Allow);
        await _overrideRepository.Received(1).UpdateAsync(existingOverride, Arg.Any<CancellationToken>());
        await _permissionCache.Received(1).InvalidateUserPermissionsAsync(user.Id.Value, Arg.Any<CancellationToken>());
    }
}
