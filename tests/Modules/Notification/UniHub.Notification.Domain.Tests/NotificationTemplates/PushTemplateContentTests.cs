using UniHub.Notification.Domain.NotificationTemplates;

namespace UniHub.Notification.Domain.Tests.NotificationTemplates;

public class PushTemplateContentTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = PushTemplateContent.Create(
            "New Message",
            "You have a new message from {SenderName}",
            "https://icon.url",
            5);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New Message");
        result.Value.Body.Should().Be("You have a new message from {SenderName}");
        result.Value.IconUrl.Should().Be("https://icon.url");
        result.Value.BadgeCount.Should().Be(5);
    }

    [Fact]
    public void Create_WithTitleAndBodyOnly_ShouldReturnSuccess()
    {
        var result = PushTemplateContent.Create("Title", "Body");

        result.IsSuccess.Should().BeTrue();
        result.Value.IconUrl.Should().BeNull();
        result.Value.BadgeCount.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldReturnFailure(string? title)
    {
        var result = PushTemplateContent.Create(title!, "Body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PushTemplateContent.EmptyTitle");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyBody_ShouldReturnFailure(string? body)
    {
        var result = PushTemplateContent.Create("Title", body!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PushTemplateContent.EmptyBody");
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldReturnFailure()
    {
        var longTitle = new string('T', PushTemplateContent.MaxTitleLength + 1);
        var result = PushTemplateContent.Create(longTitle, "Body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PushTemplateContent.TitleTooLong");
    }

    [Fact]
    public void Create_WithBodyTooLong_ShouldReturnFailure()
    {
        var longBody = new string('B', PushTemplateContent.MaxBodyLength + 1);
        var result = PushTemplateContent.Create("Title", longBody);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PushTemplateContent.BodyTooLong");
    }

    [Fact]
    public void Create_WithIconUrlTooLong_ShouldReturnFailure()
    {
        var longIconUrl = new string('I', PushTemplateContent.MaxIconUrlLength + 1);
        var result = PushTemplateContent.Create("Title", "Body", longIconUrl);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PushTemplateContent.IconUrlTooLong");
    }

    [Fact]
    public void Create_WithNegativeBadgeCount_ShouldReturnFailure()
    {
        var result = PushTemplateContent.Create("Title", "Body", null, -1);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PushTemplateContent.InvalidBadgeCount");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = PushTemplateContent.Create(
            "  Title  ",
            "  Body  ",
            "  http://icon.url  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Title");
        result.Value.Body.Should().Be("Body");
        result.Value.IconUrl.Should().Be("http://icon.url");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var result = PushTemplateContent.Create("Test Title", "Test Body");

        result.Value.ToString().Should().Be("Push: Test Title");
    }
}
