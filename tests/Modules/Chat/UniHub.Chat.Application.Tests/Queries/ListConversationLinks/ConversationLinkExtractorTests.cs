using UniHub.Chat.Application.Queries.ListConversationLinks;

namespace UniHub.Chat.Application.Tests.Queries.ListConversationLinks;

public class ConversationLinkExtractorTests
{
    [Fact]
    public void ExtractUrls_WithHttpAndWww_ShouldDedupe()
    {
        var urls = ConversationLinkExtractor.ExtractUrls(
            "Xem https://example.com/a và www.uni.edu.vn/path nhé.");

        urls.Should().HaveCount(2);
        urls.Should().Contain("https://example.com/a");
        urls.Should().Contain("https://www.uni.edu.vn/path");
    }

    [Fact]
    public void ExtractUrls_WithVietnamesePunctuation_ShouldTrimTrailing()
    {
        var urls = ConversationLinkExtractor.ExtractUrls(
            "Link: https://docs.microsoft.com/ef/core.");

        urls.Should().ContainSingle();
        urls[0].Should().Be("https://docs.microsoft.com/ef/core");
    }
}
