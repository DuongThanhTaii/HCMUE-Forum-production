using System.Net;
using FluentAssertions;

namespace UniHub.IntegrationTests;

/// <summary>
/// Smoke tests: require API reachable at UNIHUB_TEST_API_BASE_URL (default http://localhost:5034).
/// Run: docker compose up -d postgres mongodb redis api
/// Then: dotnet test tests/UniHub.IntegrationTests/UniHub.IntegrationTests.csproj --filter FullyQualifiedName~ApiSmokeTests
/// </summary>
[Trait("Category", "Integration")]
public sealed class ApiSmokeTests : IClassFixture<ApiSmokeFixture>
{
    private readonly ApiSmokeFixture _fixture;

    public ApiSmokeTests(ApiSmokeFixture fixture) => _fixture = fixture;

    private static async Task AssertNoServerError(HttpResponseMessage response, string path)
    {
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            var body = await response.Content.ReadAsStringAsync();
            var snippet = body.Length <= 500 ? body : body[..500];
            throw new Xunit.Sdk.XunitException($"GET {path} returned 500. Snippet: {snippet}");
        }
    }

    [Fact]
    public async Task Health_ShouldReturn200()
    {
        var response = await _fixture.Client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthConnections_ShouldReturn200()
    {
        var response = await _fixture.Client.GetAsync("/health/connections");
        await AssertNoServerError(response, "/health/connections");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostsList_ShouldReturn200_AndNot500()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/posts?pageNumber=1&pageSize=5");
        await AssertNoServerError(response, "/api/v1/posts");
        response.StatusCode.Should().Be(HttpStatusCode.OK, "list posts is AllowAnonymous");
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("success", "envelope should include success flag");
    }

    [Fact]
    public async Task DocumentsSearch_ShouldReturn200_AndNot500()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/documents?pageNumber=1&pageSize=5");
        await AssertNoServerError(response, "/api/v1/documents");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
        if (response.StatusCode == HttpStatusCode.OK)
        {
            var json = await response.Content.ReadAsStringAsync();
            json.Should().Contain("success");
        }
    }

    [Fact]
    public async Task DocumentsSearch_WithEmptyFacultyId_ShouldReturn400_Not500()
    {
        var response = await _fixture.Client.GetAsync(
            "/api/v1/documents?pageNumber=1&pageSize=5&facultyId=00000000-0000-0000-0000-000000000000");
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest,
            "empty faculty id is rejected by FluentValidation (400), never 500");
    }

    [Fact]
    public async Task FacultiesList_ShouldReturn200_AndNot500()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/faculties");
        await AssertNoServerError(response, "/api/v1/faculties");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CoursesList_ShouldReturn200_AndNot500()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/courses");
        await AssertNoServerError(response, "/api/v1/courses");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task TagsList_ShouldReturn200_AndNot500()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/tags?pageNumber=1&pageSize=5");
        await AssertNoServerError(response, "/api/v1/tags");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SearchPosts_ShouldReturn200_AndNot500()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/search?q=forum&pageNumber=1&pageSize=5");
        await AssertNoServerError(response, "/api/v1/search");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task JobsList_ShouldReturn200_AndNot500()
    {
        var response = await _fixture.Client.GetAsync("/api/v1/jobs?page=1&pageSize=5");
        await AssertNoServerError(response, "/api/v1/jobs");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostById_WhenSeeded_ShouldReturn200_Or404_Never500()
    {
        var id = await ApiSmokeJson.TryGetFirstPostIdAsync(_fixture.Client);
        if (id is null)
        {
            return;
        }

        var response = await _fixture.Client.GetAsync($"/api/v1/posts/{id}");
        await AssertNoServerError(response, $"/api/v1/posts/{id}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostComments_WhenSeeded_ShouldReturn200_Or404_Never500()
    {
        var id = await ApiSmokeJson.TryGetFirstPostIdAsync(_fixture.Client);
        if (id is null)
        {
            return;
        }

        var response = await _fixture.Client.GetAsync($"/api/v1/posts/{id}/comments?pageNumber=1&pageSize=5");
        await AssertNoServerError(response, $"/api/v1/posts/{id}/comments");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DocumentById_WhenSeeded_ShouldReturn200_Or404_Never500()
    {
        var id = await ApiSmokeJson.TryGetFirstDocumentIdAsync(_fixture.Client);
        if (id is null)
        {
            return;
        }

        var response = await _fixture.Client.GetAsync($"/api/v1/documents/{id}");
        await AssertNoServerError(response, $"/api/v1/documents/{id}");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }
}
