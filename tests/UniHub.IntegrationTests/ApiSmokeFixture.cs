namespace UniHub.IntegrationTests;

/// <summary>
/// Shared HTTP client for smoke tests against a running API (e.g. docker compose: api on 5034).
/// Override base URL: UNIHUB_TEST_API_BASE_URL=http://localhost:5034
/// </summary>
public sealed class ApiSmokeFixture : IDisposable
{
    public HttpClient Client { get; }

    public string BaseUrl { get; }

    public ApiSmokeFixture()
    {
        var raw = Environment.GetEnvironmentVariable("UNIHUB_TEST_API_BASE_URL")?.Trim();
        BaseUrl = string.IsNullOrEmpty(raw) ? "http://localhost:5034" : raw.TrimEnd('/');
        Client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl + "/", UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(60),
        };
    }

    public void Dispose() => Client.Dispose();
}
