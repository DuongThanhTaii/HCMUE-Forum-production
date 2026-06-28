namespace UniHub.AI.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for smart search feature.
/// </summary>
public sealed class SmartSearchSettings
{
    public const string SectionName = "AI:SmartSearch";

    /// <summary>
    /// Whether smart search is enabled.
    /// </summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Whether to use AI for query understanding.
    /// </summary>
    public bool EnableQueryUnderstanding { get; init; } = true;

    /// <summary>
    /// Whether to track search history.
    /// </summary>
    public bool EnableSearchHistory { get; init; } = true;

    /// <summary>
    /// Maximum number of results to return per page.
    /// </summary>
    public int MaxPageSize { get; init; } = 50;

    /// <summary>
    /// Default page size if not specified.
    /// </summary>
    public int DefaultPageSize { get; init; } = 10;

    /// <summary>
    /// Minimum relevance score to include in results (0.0 to 1.0).
    /// </summary>
    public double MinRelevanceScore { get; init; } = 0.3;

    /// <summary>
    /// Maximum length of search query.
    /// </summary>
    public int MaxQueryLength { get; init; } = 500;

    /// <summary>
    /// Number of suggestions to generate.
    /// </summary>
    public int DefaultSuggestionCount { get; init; } = 5;

    /// <summary>
    /// Maximum number of popular searches to track.
    /// </summary>
    public int PopularSearchesLimit { get; init; } = 20;

    /// <summary>
    /// Time window for popular searches (hours).
    /// </summary>
    public int PopularSearchesWindowHours { get; init; } = 24;

    /// <summary>
    /// Whether to highlight matched terms in snippets.
    /// </summary>
    public bool HighlightMatches { get; init; } = true;

    /// <summary>
    /// Maximum length of content snippet.
    /// </summary>
    public int SnippetLength { get; init; } = 200;

    /// <summary>
    /// Supported search types.
    /// </summary>
    public List<string> SupportedSearchTypes { get; init; } = new()
    {
        "All", "Posts", "Questions", "Articles", "Users", "Documents", "FAQs"
    };

    /// <summary>
    /// Weight factors for relevance scoring.
    /// </summary>
    public RelevanceWeights Weights { get; init; } = new();
}

/// <summary>
/// Weight factors for calculating relevance scores.
/// </summary>
public sealed class RelevanceWeights
{
    /// <summary>
    /// Weight for title match (0.0 to 1.0).
    /// </summary>
    public double TitleWeight { get; init; } = 0.4;

    /// <summary>
    /// Weight for content match (0.0 to 1.0).
    /// </summary>
    public double ContentWeight { get; init; } = 0.3;

    /// <summary>
    /// Weight for tag match (0.0 to 1.0).
    /// </summary>
    public double TagWeight { get; init; } = 0.2;

    /// <summary>
    /// Weight for recency (0.0 to 1.0).
    /// </summary>
    public double RecencyWeight { get; init; } = 0.1;

    /// <summary>
    /// Boost for exact phrase matches.
    /// </summary>
    public double ExactMatchBoost { get; init; } = 1.5;

    /// <summary>
    /// Boost for popular content (based on views).
    /// </summary>
    public double PopularityBoost { get; init; } = 1.2;
}
