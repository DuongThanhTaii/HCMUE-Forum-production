using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using UniHub.Notification.Application.Abstractions;
using UniHub.Notification.Application.Commands.UpdateNotificationPreferences;
using UniHub.Notification.Domain.NotificationPreferences;
using Xunit;

namespace UniHub.Notification.Application.Tests.Commands;

public class UpdateNotificationPreferencesCommandHandlerTests
{
    private readonly INotificationPreferenceRepository _preferenceRepository;
    private readonly ILogger<UpdateNotificationPreferencesCommandHandler> _logger;
    private readonly UpdateNotificationPreferencesCommandHandler _handler;

    public UpdateNotificationPreferencesCommandHandlerTests()
    {
        _preferenceRepository = Substitute.For<INotificationPreferenceRepository>();
        _logger = Substitute.For<ILogger<UpdateNotificationPreferencesCommandHandler>>();
        _handler = new UpdateNotificationPreferencesCommandHandler(_preferenceRepository, _logger);
    }

    [Fact]
    public async Task Handle_WhenPreferencesExist_ShouldUpdate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var preference = NotificationPreference.Create(userId).Value;
        _preferenceRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(preference);

        var command = new UpdateNotificationPreferencesCommand(userId, false, true, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        preference.EmailEnabled.Should().BeFalse();
        preference.PushEnabled.Should().BeTrue();
        preference.InAppEnabled.Should().BeTrue();
        await _preferenceRepository.Received(1).UpdateAsync(preference, Arg.Any<CancellationToken>());
        await _preferenceRepository.DidNotReceive().AddAsync(Arg.Any<NotificationPreference>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPreferencesDoNotExist_ShouldCreate()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _preferenceRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((NotificationPreference?)null);

        var command = new UpdateNotificationPreferencesCommand(userId, false, true, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _preferenceRepository.Received(1).AddAsync(Arg.Is<NotificationPreference>(p =>
            p.UserId == userId &&
            p.EmailEnabled == false &&
            p.PushEnabled == true &&
            p.InAppEnabled == false), Arg.Any<CancellationToken>());
        await _preferenceRepository.DidNotReceive().UpdateAsync(Arg.Any<NotificationPreference>(), Arg.Any<CancellationToken>());
    }
}
