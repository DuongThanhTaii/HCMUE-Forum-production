namespace UniHub.AI.Application.DTOs;

/// <summary>
/// Request model for smart search.
/// </summary>
public sealed class SearchRequest
{
    /// <summary>
    /// Search query text.
    /// </summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Type of search to perform.
    /// </summary>
    public SearchType SearchType { get; init; } = SearchType.All;

    /// <summary>
    /// Category filter (optional).
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Tags to filter by (optional).
    /// </summary>
    public List<string>? Tags { get; init; }

    /// <summary>
    /// Start date for filtering (optional).
    /// </summary>
    public DateTime? StartDate { get; init; }

    /// <summary>
    /// End date for filtering (optional).
    /// </summary>
    public DateTime? EndDate { get; init; }

    /// <summary>
    /// User ID for personalized search (optional).
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of results per page.
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Whether to include search suggestions.
    /// </summary>
    public bool IncludeSuggestions { get; init; } = true;

    /// <summary>
    /// Minimum relevance score (0.0 to 1.0).
    /// </summary>
    public double MinRelevanceScore { get; init; } = 0.5;

    /// <summary>
    /// Language preference (optional).
    /// </summary>
    public string? Language { get; init; }
}

/// <summary>
/// Type of search to perform.
/// </summary>
public enum SearchType
{
    /// <summary>
    /// Search all content types.
    /// </summary>
    All = 0,

    /// <summary>
    /// Search forum posts only.
    /// </summary>
    Posts = 1,

    /// <summary>
    /// Search questions only.
    /// </summary>
    Questions = 2,

    /// <summary>
    /// Search articles only.
    /// </summary>
    Articles = 3,

    /// <summary>
    /// Search users only.
    /// </summary>
    Users = 4,

    /// <summary>
    /// Search documents only.
    /// </summary>
    Documents = 5,

    /// <summary>
    /// Search FAQ items only.
    /// </summary>
    FAQs = 6
}
