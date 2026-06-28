using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Commands.Authorization.RevokeGroupPermissionOverride;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;

namespace UniHub.Identity.Application.Tests.Commands.Authorization;

public sealed class RevokeGroupPermissionOverrideCommandHandlerTests
{
    private readonly IGroupPermissionOverrideRepository _overrideRepository;
    private readonly IPermissionCache _permissionCache;
    private readonly RevokeGroupPermissionOverrideCommandHandler _handler;

    public RevokeGroupPermissionOverrideCommandHandlerTests()
    {
        _overrideRepository = Substitute.For<IGroupPermissionOverrideRepository>();
        _permissionCache = Substitute.For<IPermissionCache>();

        _handler = new RevokeGroupPermissionOverrideCommandHandler(
            _overrideRepository,
            _permissionCache);
    }

    [Fact]
    public async Task Handle_WhenOverrideExists_ShouldRevokeSuccessfully()
    {
        var permission = Permission.Create("forum.post.create", "Create post").Value;
        var overrideItem = GroupPermissionOverride.Create(
            Guid.NewGuid(),
            permission.Id,
            PermissionScope.Global(),
            PermissionEffect.Allow,
            "Temporary allow").Value;

        _overrideRepository.GetByKeyAsync(
                overrideItem.GroupId,
                permission.Id,
                Arg.Any<PermissionScope>(),
                Arg.Any<CancellationToken>())
            .Returns(overrideItem);

        var command = new RevokeGroupPermissionOverrideCommand(
            overrideItem.GroupId,
            permission.Id.Value,
            "Global",
            null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        overrideItem.IsRevoked.Should().BeTrue();
        await _overrideRepository.Received(1).UpdateAsync(overrideItem, Arg.Any<CancellationToken>());
        await _permissionCache.Received(1).InvalidateAllAsync(Arg.Any<CancellationToken>());
    }
}
