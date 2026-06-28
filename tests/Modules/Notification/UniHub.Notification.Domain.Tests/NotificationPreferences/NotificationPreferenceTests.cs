using FluentAssertions;
using UniHub.Notification.Domain.NotificationPreferences;

namespace UniHub.Notification.Domain.Tests.NotificationPreferences;

public class NotificationPreferenceTests
{
    [Fact]
    public void Create_WithValidUserId_ShouldReturnSuccess()
    {
        var userId = Guid.NewGuid();

        var result = NotificationPreference.Create(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.EmailEnabled.Should().BeTrue();
        result.Value.PushEnabled.Should().BeTrue();
        result.Value.InAppEnabled.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyUserId_ShouldReturnFailure()
    {
        var result = NotificationPreference.Create(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(NotificationPreferenceErrors.UserIdEmpty);
    }

    [Fact]
    public void UpdateEmailPreference_ShouldUpdateAndRaiseEvent()
    {
        var preference = NotificationPreference.Create(Guid.NewGuid()).Value;
        preference.ClearDomainEvents();

        preference.UpdateEmailPreference(false);

        preference.EmailEnabled.Should().BeFalse();
        preference.UpdatedAt.Should().NotBeNull();
        preference.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void UpdatePushPreference_ShouldUpdateAndRaiseEvent()
    {
        var preference = NotificationPreference.Create(Guid.NewGuid()).Value;
        preference.ClearDomainEvents();

        preference.UpdatePushPreference(false);

        preference.PushEnabled.Should().BeFalse();
        preference.UpdatedAt.Should().NotBeNull();
        preference.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void UpdateInAppPreference_ShouldUpdateAndRaiseEvent()
    {
        var preference = NotificationPreference.Create(Guid.NewGuid()).Value;
        preference.ClearDomainEvents();

        preference.UpdateInAppPreference(false);

        preference.InAppEnabled.Should().BeFalse();
        preference.UpdatedAt.Should().NotBeNull();
        preference.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void EnableAll_ShouldEnableAllChannels()
    {
        var preference = NotificationPreference.Create(Guid.NewGuid()).Value;
        preference.DisableAll();
        preference.ClearDomainEvents();

        preference.EnableAll();

        preference.EmailEnabled.Should().BeTrue();
        preference.PushEnabled.Should().BeTrue();
        preference.InAppEnabled.Should().BeTrue();
    }

    [Fact]
    public void DisableAll_ShouldDisableAllChannels()
    {
        var preference = NotificationPreference.Create(Guid.NewGuid()).Value;
        preference.ClearDomainEvents();

        preference.DisableAll();

        preference.EmailEnabled.Should().BeFalse();
        preference.PushEnabled.Should().BeFalse();
        preference.InAppEnabled.Should().BeFalse();
    }
}
