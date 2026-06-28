using UniHub.Notification.Domain.NotificationTemplates;

namespace UniHub.Notification.Domain.Tests.NotificationTemplates;

public class InAppTemplateContentTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = InAppTemplateContent.Create(
            "Comment Received",
            "{UserName} commented on your post: {CommentText}",
            "/posts/123",
            "https://icon.url");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Comment Received");
        result.Value.Body.Should().Be("{UserName} commented on your post: {CommentText}");
        result.Value.ActionUrl.Should().Be("/posts/123");
        result.Value.IconUrl.Should().Be("https://icon.url");
    }

    [Fact]
    public void Create_WithTitleAndBodyOnly_ShouldReturnSuccess()
    {
        var result = InAppTemplateContent.Create("Title", "Body");

        result.IsSuccess.Should().BeTrue();
        result.Value.ActionUrl.Should().BeNull();
        result.Value.IconUrl.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldReturnFailure(string? title)
    {
        var result = InAppTemplateContent.Create(title!, "Body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InAppTemplateContent.EmptyTitle");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyBody_ShouldReturnFailure(string? body)
    {
        var result = InAppTemplateContent.Create("Title", body!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InAppTemplateContent.EmptyBody");
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldReturnFailure()
    {
        var longTitle = new string('T', InAppTemplateContent.MaxTitleLength + 1);
        var result = InAppTemplateContent.Create(longTitle, "Body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InAppTemplateContent.TitleTooLong");
    }

    [Fact]
    public void Create_WithBodyTooLong_ShouldReturnFailure()
    {
        var longBody = new string('B', InAppTemplateContent.MaxBodyLength + 1);
        var result = InAppTemplateContent.Create("Title", longBody);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InAppTemplateContent.BodyTooLong");
    }

    [Fact]
    public void Create_WithActionUrlTooLong_ShouldReturnFailure()
    {
        var longActionUrl = new string('A', InAppTemplateContent.MaxActionUrlLength + 1);
        var result = InAppTemplateContent.Create("Title", "Body", longActionUrl);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InAppTemplateContent.ActionUrlTooLong");
    }

    [Fact]
    public void Create_WithIconUrlTooLong_ShouldReturnFailure()
    {
        var longIconUrl = new string('I', InAppTemplateContent.MaxIconUrlLength + 1);
        var result = InAppTemplateContent.Create("Title", "Body", null, longIconUrl);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InAppTemplateContent.IconUrlTooLong");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = InAppTemplateContent.Create(
            "  Title  ",
            "  Body  ",
            "  /action  ",
            "  http://icon.url  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Title");
        result.Value.Body.Should().Be("Body");
        result.Value.ActionUrl.Should().Be("/action");
        result.Value.IconUrl.Should().Be("http://icon.url");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var result = InAppTemplateContent.Create("Test Title", "Test Body");

        result.Value.ToString().Should().Be("InApp: Test Title");
    }
}
