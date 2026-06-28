namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Response model for smart search.
/// </summary>
public sealed class SearchResponse
{
    /// <summary>
    /// Search results.
    /// </summary>
    public List<SearchResult> Results { get; init; } = new();

    /// <summary>
    /// Total number of results (before pagination).
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// Number of results per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Search suggestions based on query.
    /// </summary>
    public List<string> Suggestions { get; init; } = new();

    /// <summary>
    /// AI-enhanced query understanding.
    /// </summary>
    public QueryUnderstanding? QueryUnderstanding { get; init; }

    /// <summary>
    /// Time taken for search (milliseconds).
    /// </summary>
    public long ProcessingTimeMs { get; init; }

    /// <summary>
    /// Timestamp of search.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Individual search result.
/// </summary>
public sealed class SearchResult
{
    /// <summary>
    /// Unique identifier of the result.
    /// </summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>
    /// Type of content.
    /// </summary>
    public string ContentType { get; init; } = string.Empty;

    /// <summary>
    /// Title of the result.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Content snippet with highlighted matches.
    /// </summary>
    public string Snippet { get; init; } = string.Empty;

    /// <summary>
    /// Relevance score (0.0 to 1.0).
    /// </summary>
    public double RelevanceScore { get; init; }

    /// <summary>
    /// URL or path to the content.
    /// </summary>
    public string Url { get; init; } = string.Empty;

    /// <summary>
    /// Author or creator name.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Creation date of content.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Category of content.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Tags associated with content.
    /// </summary>
    public List<string>? Tags { get; init; }

    /// <summary>
    /// Number of views/interactions.
    /// </summary>
    public int? ViewCount { get; init; }

    /// <summary>
    /// Additional metadata as key-value pairs.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; init; }
}

/// <summary>
/// AI-powered query understanding.
/// </summary>
public sealed class QueryUnderstanding
{
    /// <summary>
    /// Original query as entered.
    /// </summary>
    public string OriginalQuery { get; init; } = string.Empty;

    /// <summary>
    /// Expanded query with synonyms and related terms.
    /// </summary>
    public string ExpandedQuery { get; init; } = string.Empty;

    /// <summary>
    /// Detected intent of the query.
    /// </summary>
    public string Intent { get; init; } = string.Empty;

    /// <summary>
    /// Extracted entities from query (names, dates, etc.).
    /// </summary>
    public List<string> Entities { get; init; } = new();

    /// <summary>
    /// Detected language of query.
    /// </summary>
    public string Language { get; init; } = "en";

    /// <summary>
    /// Suggested corrections if query has typos.
    /// </summary>
    public string? SuggestedCorrection { get; init; }
}
