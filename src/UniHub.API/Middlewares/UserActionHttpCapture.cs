using System.Text;
using System.Text.Json;

namespace UniHub.API.Middlewares;

internal static class UserActionHttpCapture
{
    private static readonly HashSet<string> RedactedHeaderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key",
        "Proxy-Authorization",
    };

    public static string SerializeRequestHeaders(IHeaderDictionary headers, int maxJsonLength)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in headers)
        {
            var value = pair.Value.ToString();
            if (RedactedHeaderNames.Contains(pair.Key))
            {
                value = string.IsNullOrEmpty(value) ? string.Empty : "[REDACTED]";
            }

            if (value.Length > 512)
            {
                value = value[..512] + "…";
            }

            dict[pair.Key] = value;
        }

        var json = JsonSerializer.Serialize(dict);
        return json.Length <= maxJsonLength ? json : json[..maxJsonLength] + "…";
    }

    public static string SerializeResponseHeaders(IHeaderDictionary headers, int maxJsonLength)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in headers)
        {
            var value = pair.Value.ToString();
            if (RedactedHeaderNames.Contains(pair.Key))
            {
                value = string.IsNullOrEmpty(value) ? string.Empty : "[REDACTED]";
            }

            if (value.Length > 512)
            {
                value = value[..512] + "…";
            }

            dict[pair.Key] = value;
        }

        var json = JsonSerializer.Serialize(dict);
        return json.Length <= maxJsonLength ? json : json[..maxJsonLength] + "…";
    }

    public static bool ShouldCaptureRequestBody(HttpRequest request)
    {
        if (HttpMethods.IsGet(request.Method) || HttpMethods.IsHead(request.Method) || HttpMethods.IsDelete(request.Method))
        {
            return false;
        }

        var ct = request.ContentType ?? string.Empty;
        if (ct.Contains("multipart/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (ct.Contains("application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    public static async Task<(string? Preview, bool Truncated)> ReadBodyPreviewAsync(
        Stream stream,
        int maxBytes,
        CancellationToken cancellationToken)
    {
        if (!stream.CanRead)
        {
            return (null, false);
        }

        using var ms = new MemoryStream();
        var buffer = new byte[8192];
        var total = 0;
        while (total < maxBytes)
        {
            var toRead = Math.Min(buffer.Length, maxBytes - total);
            var read = await stream.ReadAsync(buffer.AsMemory(0, toRead), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                break;
            }

            ms.Write(buffer, 0, read);
            total += read;
        }

        var bytes = ms.ToArray();
        var truncated = total >= maxBytes;

        try
        {
            var text = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrEmpty(text))
            {
                return (null, false);
            }

            return (text, truncated);
        }
        catch (DecoderFallbackException)
        {
            return ("[Binary body omitted]", false);
        }
    }

    /// <summary>Reads up to <paramref name="maxPreviewBytes"/> from the start of <paramref name="buffer"/> for UTF-8 text preview. Resets buffer position to 0.</summary>
    public static (string? Preview, bool TruncatedPreview) ReadResponsePreview(MemoryStream buffer, int maxPreviewBytes)
    {
        if (buffer.Length == 0)
        {
            return (null, false);
        }

        var fullLen = (int)buffer.Length;
        var take = Math.Min(fullLen, maxPreviewBytes);
        buffer.Position = 0;
        var rent = new byte[take];
        _ = buffer.Read(rent, 0, take);
        buffer.Position = 0;
        var truncated = fullLen > maxPreviewBytes;
        try
        {
            var text = Encoding.UTF8.GetString(rent);
            return (string.IsNullOrEmpty(text) ? null : text, truncated);
        }
        catch (DecoderFallbackException)
        {
            return ("[Binary response omitted]", truncated);
        }
    }
}
