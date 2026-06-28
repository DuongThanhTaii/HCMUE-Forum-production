using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Commands.Authorization.RevokeUserPermissionOverride;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Application.Tests.Commands.Authorization;

public sealed class RevokeUserPermissionOverrideCommandHandlerTests
{
    private readonly IUserPermissionOverrideRepository _overrideRepository;
    private readonly IPermissionCache _permissionCache;
    private readonly RevokeUserPermissionOverrideCommandHandler _handler;

    public RevokeUserPermissionOverrideCommandHandlerTests()
    {
        _overrideRepository = Substitute.For<IUserPermissionOverrideRepository>();
        _permissionCache = Substitute.For<IPermissionCache>();

        _handler = new RevokeUserPermissionOverrideCommandHandler(
            _overrideRepository,
            _permissionCache);
    }

    [Fact]
    public async Task Handle_WhenOverrideExists_ShouldRevokeSuccessfully()
    {
        var user = User.Create(
            Email.Create("revoke-user@example.com").Value,
            "hashed-password",
            UserProfile.Create("Revoke", "User").Value).Value;

        var permission = Permission.Create("forum.post.create", "Create post").Value;
        var existingOverride = UserPermissionOverride.Create(
            user.Id,
            permission.Id,
            PermissionScope.Global(),
            PermissionEffect.Allow,
            "Temporary allow").Value;

        _overrideRepository.GetByKeyAsync(
                user.Id,
                permission.Id,
                Arg.Any<PermissionScope>(),
                Arg.Any<CancellationToken>())
            .Returns(existingOverride);

        var command = new RevokeUserPermissionOverrideCommand(
            user.Id.Value,
            permission.Id.Value,
            "Global",
            null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existingOverride.IsRevoked.Should().BeTrue();
        await _overrideRepository.Received(1).UpdateAsync(existingOverride, Arg.Any<CancellationToken>());
        await _permissionCache.Received(1).InvalidateUserPermissionsAsync(user.Id.Value, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOverrideNotFound_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var permissionId = Guid.NewGuid();

        _overrideRepository.GetByKeyAsync(
                Arg.Any<UserId>(),
                Arg.Any<PermissionId>(),
                Arg.Any<PermissionScope>(),
                Arg.Any<CancellationToken>())
            .Returns((UserPermissionOverride?)null);

        var command = new RevokeUserPermissionOverrideCommand(
            userId,
            permissionId,
            "Global",
            null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserPermissionOverride.NotFound");
        await _overrideRepository.DidNotReceive().UpdateAsync(Arg.Any<UserPermissionOverride>(), Arg.Any<CancellationToken>());
        await _permissionCache.DidNotReceive().InvalidateUserPermissionsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
