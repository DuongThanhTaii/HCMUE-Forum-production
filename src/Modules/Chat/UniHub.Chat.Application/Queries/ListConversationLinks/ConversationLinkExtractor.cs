using System.Text.RegularExpressions;

namespace UniHub.Chat.Application.Queries.ListConversationLinks;

public static partial class ConversationLinkExtractor
{
    [GeneratedRegex(
        @"https?://[^\s<>""')\]]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 200)]
    private static partial Regex HttpUrlRegex();

    [GeneratedRegex(
        @"\bwww\.[^\s<>""')\]]+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 200)]
    private static partial Regex WwwUrlRegex();

    public static IReadOnlyList<string> ExtractUrls(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return [];
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var urls = new List<string>();

        void AddMatches(Regex regex, bool prependHttps = false)
        {
            foreach (Match match in regex.Matches(content))
            {
                var raw = match.Value.TrimEnd('.', ',', ';', ':', '!', '?', ')', ']', '}', '"', '\'');
                if (raw.Length == 0)
                {
                    continue;
                }

                var url = prependHttps ? $"https://{raw}" : raw;
                if (seen.Add(url))
                {
                    urls.Add(url);
                }
            }
        }

        AddMatches(HttpUrlRegex());
        AddMatches(WwwUrlRegex(), prependHttps: true);

        return urls;
    }
}
