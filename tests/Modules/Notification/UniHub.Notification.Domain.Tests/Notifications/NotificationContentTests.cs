using UniHub.Notification.Domain.Notifications;

namespace UniHub.Notification.Domain.Tests.Notifications;

public class NotificationContentTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = NotificationContent.Create(
            "New Message",
            "You have received a new message from John",
            "/messages/123",
            "https://icon.url");

        result.IsSuccess.Should().BeTrue();
        result.Value.Subject.Should().Be("New Message");
        result.Value.Body.Should().Be("You have received a new message from John");
        result.Value.ActionUrl.Should().Be("/messages/123");
        result.Value.IconUrl.Should().Be("https://icon.url");
    }

    [Fact]
    public void Create_WithSubjectAndBodyOnly_ShouldReturnSuccess()
    {
        var result = NotificationContent.Create("Subject", "Body");

        result.IsSuccess.Should().BeTrue();
        result.Value.Subject.Should().Be("Subject");
        result.Value.Body.Should().Be("Body");
        result.Value.ActionUrl.Should().BeNull();
        result.Value.IconUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptySubject_ShouldReturnFailure(string? subject)
    {
        var result = NotificationContent.Create(subject!, "Body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationContent.EmptySubject");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyBody_ShouldReturnFailure(string? body)
    {
        var result = NotificationContent.Create("Subject", body!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationContent.EmptyBody");
    }

    [Fact]
    public void Create_WithSubjectTooLong_ShouldReturnFailure()
    {
        var longSubject = new string('S', NotificationContent.MaxSubjectLength + 1);
        var result = NotificationContent.Create(longSubject, "Body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationContent.SubjectTooLong");
    }

    [Fact]
    public void Create_WithBodyTooLong_ShouldReturnFailure()
    {
        var longBody = new string('B', NotificationContent.MaxBodyLength + 1);
        var result = NotificationContent.Create("Subject", longBody);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationContent.BodyTooLong");
    }

    [Fact]
    public void Create_WithActionUrlTooLong_ShouldReturnFailure()
    {
        var longActionUrl = new string('A', NotificationContent.MaxActionUrlLength + 1);
        var result = NotificationContent.Create("Subject", "Body", longActionUrl);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationContent.ActionUrlTooLong");
    }

    [Fact]
    public void Create_WithIconUrlTooLong_ShouldReturnFailure()
    {
        var longIconUrl = new string('I', NotificationContent.MaxIconUrlLength + 1);
        var result = NotificationContent.Create("Subject", "Body", null, longIconUrl);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NotificationContent.IconUrlTooLong");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = NotificationContent.Create(
            "  Subject  ",
            "  Body  ",
            "  /action  ",
            "  https://icon.url  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Subject.Should().Be("Subject");
        result.Value.Body.Should().Be("Body");
        result.Value.ActionUrl.Should().Be("/action");
        result.Value.IconUrl.Should().Be("https://icon.url");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var result = NotificationContent.Create("Test", "This is a long body text that should be truncated");

        result.Value.ToString().Should().Contain("Test:");
        result.Value.ToString().Should().Contain("...");
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var content1 = NotificationContent.Create("Subject", "Body", "/action").Value;
        var content2 = NotificationContent.Create("Subject", "Body", "/action").Value;

        content1.Should().Be(content2);
        (content1 == content2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        var content1 = NotificationContent.Create("Subject1", "Body1").Value;
        var content2 = NotificationContent.Create("Subject2", "Body2").Value;

        content1.Should().NotBe(content2);
        (content1 != content2).Should().BeTrue();
    }
}
