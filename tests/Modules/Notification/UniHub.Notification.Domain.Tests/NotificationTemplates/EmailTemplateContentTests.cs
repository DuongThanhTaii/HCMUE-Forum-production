using UniHub.Notification.Domain.NotificationTemplates;

namespace UniHub.Notification.Domain.Tests.NotificationTemplates;

public class EmailTemplateContentTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var result = EmailTemplateContent.Create(
            "Welcome to {AppName}",
            "Hello {UserName}, welcome to our platform!",
            "UniHub Team",
            "noreply@unihub.com");

        result.IsSuccess.Should().BeTrue();
        result.Value.Subject.Should().Be("Welcome to {AppName}");
        result.Value.Body.Should().Be("Hello {UserName}, welcome to our platform!");
        result.Value.FromName.Should().Be("UniHub Team");
        result.Value.FromEmail.Should().Be("noreply@unihub.com");
    }

    [Fact]
    public void Create_WithSubjectAndBodyOnly_ShouldReturnSuccess()
    {
        var result = EmailTemplateContent.Create(
            "Test Subject",
            "Test Body");

        result.IsSuccess.Should().BeTrue();
        result.Value.Subject.Should().Be("Test Subject");
        result.Value.Body.Should().Be("Test Body");
        result.Value.FromName.Should().BeNull();
        result.Value.FromEmail.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptySubject_ShouldReturnFailure(string? subject)
    {
        var result = EmailTemplateContent.Create(subject!, "Body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailTemplateContent.EmptySubject");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyBody_ShouldReturnFailure(string? body)
    {
        var result = EmailTemplateContent.Create("Subject", body!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailTemplateContent.EmptyBody");
    }

    [Fact]
    public void Create_WithSubjectTooLong_ShouldReturnFailure()
    {
        var longSubject = new string('A', EmailTemplateContent.MaxSubjectLength + 1);
        var result = EmailTemplateContent.Create(longSubject, "Body");

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailTemplateContent.SubjectTooLong");
    }

    [Fact]
    public void Create_WithBodyTooLong_ShouldReturnFailure()
    {
        var longBody = new string('B', EmailTemplateContent.MaxBodyLength + 1);
        var result = EmailTemplateContent.Create("Subject", longBody);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailTemplateContent.BodyTooLong");
    }

    [Fact]
    public void Create_WithFromNameTooLong_ShouldReturnFailure()
    {
        var longFromName = new string('N', EmailTemplateContent.MaxFromNameLength + 1);
        var result = EmailTemplateContent.Create(
            "Subject",
            "Body",
            longFromName);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailTemplateContent.FromNameTooLong");
    }

    [Fact]
    public void Create_WithFromEmailTooLong_ShouldReturnFailure()
    {
        var longFromEmail = new string('E', EmailTemplateContent.MaxFromEmailLength + 1);
        var result = EmailTemplateContent.Create(
            "Subject",
            "Body",
            "Name",
            longFromEmail);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EmailTemplateContent.FromEmailTooLong");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = EmailTemplateContent.Create(
            "  Subject  ",
            "  Body  ",
            "  Name  ",
            "  email@test.com  ");

        result.IsSuccess.Should().BeTrue();
        result.Value.Subject.Should().Be("Subject");
        result.Value.Body.Should().Be("Body");
        result.Value.FromName.Should().Be("Name");
        result.Value.FromEmail.Should().Be("email@test.com");
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString()
    {
        var result = EmailTemplateContent.Create("Test Subject", "Test Body");

        result.Value.ToString().Should().Be("Email: Test Subject");
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        var content1 = EmailTemplateContent.Create(
            "Subject",
            "Body",
            "Name",
            "email@test.com").Value;

        var content2 = EmailTemplateContent.Create(
            "Subject",
            "Body",
            "Name",
            "email@test.com").Value;

        content1.Should().Be(content2);
        (content1 == content2).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        var content1 = EmailTemplateContent.Create("Subject1", "Body1").Value;
        var content2 = EmailTemplateContent.Create("Subject2", "Body2").Value;

        content1.Should().NotBe(content2);
        (content1 != content2).Should().BeTrue();
    }

    #endregion
}
