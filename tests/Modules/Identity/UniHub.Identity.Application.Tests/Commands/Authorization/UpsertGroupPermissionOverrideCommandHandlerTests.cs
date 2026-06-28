using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Commands.Authorization.UpsertGroupPermissionOverride;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Domain.Permissions;

namespace UniHub.Identity.Application.Tests.Commands.Authorization;

public sealed class UpsertGroupPermissionOverrideCommandHandlerTests
{
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IGroupPermissionOverrideRepository _overrideRepository;
    private readonly IPermissionCache _permissionCache;
    private readonly UpsertGroupPermissionOverrideCommandHandler _handler;

    public UpsertGroupPermissionOverrideCommandHandlerTests()
    {
        _userGroupRepository = Substitute.For<IUserGroupRepository>();
        _permissionRepository = Substitute.For<IPermissionRepository>();
        _overrideRepository = Substitute.For<IGroupPermissionOverrideRepository>();
        _permissionCache = Substitute.For<IPermissionCache>();

        _handler = new UpsertGroupPermissionOverrideCommandHandler(
            _userGroupRepository,
            _permissionRepository,
            _overrideRepository,
            _permissionCache);
    }

    [Fact]
    public async Task Handle_WhenGroupDoesNotExist_ShouldReturnFailure()
    {
        var command = new UpsertGroupPermissionOverrideCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Global",
            null,
            "Allow",
            null,
            null);

        _userGroupRepository.GetByIdAsync(command.GroupId, Arg.Any<CancellationToken>())
            .Returns((UserGroup?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserGroup.NotFound");
    }

    [Fact]
    public async Task Handle_WhenValidCommand_ShouldCreateOverride()
    {
        var group = UserGroup.Create("moderators").Value;
        var permission = Permission.Create("forum.post.create", "Create post").Value;

        var command = new UpsertGroupPermissionOverrideCommand(
            group.Id,
            permission.Id.Value,
            "Global",
            null,
            "Deny",
            "Group restricted",
            null);

        _userGroupRepository.GetByIdAsync(group.Id, Arg.Any<CancellationToken>())
            .Returns(group);
        _permissionRepository.GetByIdAsync(permission.Id, Arg.Any<CancellationToken>())
            .Returns(permission);
        _overrideRepository.GetByKeyAsync(group.Id, permission.Id, Arg.Any<PermissionScope>(), Arg.Any<CancellationToken>())
            .Returns((GroupPermissionOverride?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _overrideRepository.Received(1).AddAsync(Arg.Any<GroupPermissionOverride>(), Arg.Any<CancellationToken>());
        await _permissionCache.Received(1).InvalidateAllAsync(Arg.Any<CancellationToken>());
    }
}
