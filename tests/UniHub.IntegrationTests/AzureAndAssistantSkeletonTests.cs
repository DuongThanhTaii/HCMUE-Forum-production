using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;

namespace UniHub.IntegrationTests;

[Trait("Category", "Integration")]
public sealed class AzureAndAssistantSkeletonTests : IClassFixture<ApiSmokeFixture>
{
    private readonly ApiSmokeFixture _fixture;

    public AzureAndAssistantSkeletonTests(ApiSmokeFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task AzureBearerToken_WhenProvided_ShouldAccessProtectedEndpoint()
    {
        var azureBearer = Environment.GetEnvironmentVariable("UNIHUB_TEST_AZURE_BEARER_TOKEN")?.Trim();
        if (string.IsNullOrWhiteSpace(azureBearer))
        {
            return;
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, "/auth/test");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", azureBearer);
        var response = await _fixture.Client.SendAsync(request);

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "valid Azure token should pass DynamicJwt -> Azure AD scheme path");
    }

    [Fact]
    public async Task AssistantTools_WithBearerToken_ShouldReturnNon500()
    {
        var bearer = Environment.GetEnvironmentVariable("UNIHUB_TEST_BEARER_TOKEN")?.Trim();
        if (string.IsNullOrWhiteSpace(bearer))
        {
            return;
        }

        var postId = Environment.GetEnvironmentVariable("UNIHUB_TEST_POST_ID")?.Trim();
        if (string.IsNullOrWhiteSpace(postId))
        {
            var seededPostId = await ApiSmokeJson.TryGetFirstPostIdAsync(_fixture.Client);
            postId = seededPostId?.ToString();
        }

        if (string.IsNullOrWhiteSpace(postId))
        {
            return;
        }

        await AssertAssistantEndpoint("/api/v1/assistant-tools/summarize-post", bearer, new
        {
            postId,
            maxSentences = 4,
        });

        await AssertAssistantEndpoint("/api/v1/assistant-tools/related-posts", bearer, new
        {
            postId,
            topK = 5,
        });

        await AssertAssistantEndpoint("/api/v1/assistant-tools/draft-reply", bearer, new
        {
            postId,
            tone = "neutral",
            maxTokens = 280,
        });

        await AssertAssistantEndpoint("/api/v1/assistant-tools/moderation-hint", bearer, new
        {
            postId,
        });

        await AssertAssistantEndpoint("/api/v1/assistant-tools/suggest-title-tags", bearer, new
        {
            contentMarkdown = "This is a test post body for assistant title and tags suggestion.",
            existingTags = Array.Empty<string>(),
        });

        await AssertAssistantEndpoint("/api/v1/assistant-tools/rewrite-content", bearer, new
        {
            contentMarkdown = "This is a short forum draft. Please keep meaning and improve clarity.",
            style = "clear",
            maxWords = 200,
        });
    }

    private async Task AssertAssistantEndpoint(string path, string bearerToken, object payload)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        var response = await _fixture.Client.SendAsync(request);

        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError, $"{path} should not crash API");
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden);
    }
}
