using System.Net.Http;
using System.Text.Json;

namespace UniHub.IntegrationTests;

internal static class ApiSmokeJson
{
    public static async Task<Guid?> TryGetFirstPostIdAsync(HttpClient client, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync("/api/v1/posts?pageNumber=1&pageSize=1", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!doc.RootElement.TryGetProperty("data", out var data))
        {
            return null;
        }

        if (!data.TryGetProperty("posts", out var posts) || posts.ValueKind != JsonValueKind.Array || posts.GetArrayLength() == 0)
        {
            return null;
        }

        var id = posts[0].GetProperty("id");
        return id.GetGuid();
    }

    public static async Task<Guid?> TryGetFirstDocumentIdAsync(HttpClient client, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync("/api/v1/documents?pageNumber=1&pageSize=1", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!doc.RootElement.TryGetProperty("data", out var data))
        {
            return null;
        }

        if (!data.TryGetProperty("documents", out var documents) ||
            documents.ValueKind != JsonValueKind.Array ||
            documents.GetArrayLength() == 0)
        {
            return null;
        }

        var id = documents[0].GetProperty("id");
        return id.GetGuid();
    }
}
