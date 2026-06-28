namespace UniHub.Chat.Application.Queries.SearchConversationMessages;

internal static class MessageSearchSnippet
{
    public static string Build(string content, string query, int maxLen = 140)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var trimmed = content.Trim();
        if (trimmed.Length <= maxLen)
        {
            return trimmed;
        }

        var idx = trimmed.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return trimmed[..maxLen] + "…";
        }

        var half = Math.Max(20, (maxLen - query.Length) / 2);
        var start = Math.Max(0, idx - half);
        var end = Math.Min(trimmed.Length, idx + query.Length + half);
        var slice = trimmed[start..end];
        if (start > 0)
        {
            slice = "…" + slice;
        }

        if (end < trimmed.Length)
        {
            slice += "…";
        }

        return slice;
    }
}
