using FluentAssertions;
using UniHub.Career.Domain.Companies;

namespace UniHub.Career.Domain.Tests.Companies;

public class SocialLinksTests
{
    [Fact]
    public void Create_WithAllLinks_ShouldReturnSuccess()
    {
        var result = SocialLinks.Create(
            "https://linkedin.com/company/test",
            "https://facebook.com/test",
            "https://twitter.com/test",
            "https://instagram.com/test",
            "https://youtube.com/test");

        result.IsSuccess.Should().BeTrue();
        result.Value.LinkedIn.Should().Be("https://linkedin.com/company/test");
        result.Value.Facebook.Should().Be("https://facebook.com/test");
        result.Value.Twitter.Should().Be("https://twitter.com/test");
        result.Value.Instagram.Should().Be("https://instagram.com/test");
        result.Value.YouTube.Should().Be("https://youtube.com/test");
    }

    [Fact]
    public void Create_WithNoLinks_ShouldReturnSuccess()
    {
        var result = SocialLinks.Create();

        result.IsSuccess.Should().BeTrue();
        result.Value.LinkedIn.Should().BeNull();
        result.Value.Facebook.Should().BeNull();
        result.Value.Twitter.Should().BeNull();
        result.Value.Instagram.Should().BeNull();
        result.Value.YouTube.Should().BeNull();
    }

    [Fact]
    public void Create_WithLinkedInTooLong_ShouldReturnFailure()
    {
        var longUrl = "https://" + new string('a', SocialLinks.MaxUrlLength);
        var result = SocialLinks.Create(linkedIn: longUrl);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SocialLinks.LinkedInTooLong");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://test.com")]
    [InlineData("//test.com")]
    public void Create_WithInvalidLinkedInUrl_ShouldReturnFailure(string invalidUrl)
    {
        var result = SocialLinks.Create(linkedIn: invalidUrl);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SocialLinks.InvalidLinkedIn");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://test.com")]
    public void Create_WithInvalidFacebookUrl_ShouldReturnFailure(string invalidUrl)
    {
        var result = SocialLinks.Create(facebook: invalidUrl);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SocialLinks.InvalidFacebook");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://test.com")]
    public void Create_WithInvalidTwitterUrl_ShouldReturnFailure(string invalidUrl)
    {
        var result = SocialLinks.Create(twitter: invalidUrl);
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("SocialLinks.InvalidTwitter");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = SocialLinks.Create("  https://linkedin.com/test  ");
        result.IsSuccess.Should().BeTrue();
        result.Value.LinkedIn.Should().Be("https://linkedin.com/test");
    }

    [Fact]
    public void Empty_ShouldCreateEmptySocialLinks()
    {
        var links = SocialLinks.Empty();

        links.LinkedIn.Should().BeNull();
        links.Facebook.Should().BeNull();
        links.Twitter.Should().BeNull();
        links.Instagram.Should().BeNull();
        links.YouTube.Should().BeNull();
    }

    [Fact]
    public void HasAnyLinks_WithLinks_ShouldReturnTrue()
    {
        var links = SocialLinks.Create("https://linkedin.com/test").Value;
        links.HasAnyLinks().Should().BeTrue();
    }

    [Fact]
    public void HasAnyLinks_WithNoLinks_ShouldReturnFalse()
    {
        var links = SocialLinks.Empty();
        links.HasAnyLinks().Should().BeFalse();
    }

    [Fact]
    public void Equality_SameSocialLinks_ShouldBeEqual()
    {
        var s1 = SocialLinks.Create("https://linkedin.com/test").Value;
        var s2 = SocialLinks.Create("https://linkedin.com/test").Value;
        s1.Should().Be(s2);
    }

    [Fact]
    public void Equality_DifferentSocialLinks_ShouldNotBeEqual()
    {
        var s1 = SocialLinks.Create("https://linkedin.com/test1").Value;
        var s2 = SocialLinks.Create("https://linkedin.com/test2").Value;
        s1.Should().NotBe(s2);
    }
}
