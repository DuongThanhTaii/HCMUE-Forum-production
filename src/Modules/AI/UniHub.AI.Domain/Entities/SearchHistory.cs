namespace UniHub.AI.Domain.Entities;

/// <summary>
/// Entity representing a search query in history.
/// </summary>
public sealed class SearchHistory
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public string Id { get; init; } = Guid.NewGuid().ToString();

    /// <summary>
    /// User ID who performed the search (optional for anonymous).
    /// </summary>
    public string? UserId { get; init; }

    /// <summary>
    /// Search query text.
    /// </summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Normalized query for matching similar searches.
    /// </summary>
    public string NormalizedQuery { get; init; } = string.Empty;

    /// <summary>
    /// Type of search performed.
    /// </summary>
    public string SearchType { get; init; } = string.Empty;

    /// <summary>
    /// Number of results returned.
    /// </summary>
    public int ResultCount { get; init; }

    /// <summary>
    /// Whether user clicked any result.
    /// </summary>
    public bool HadClickthrough { get; set; }

    /// <summary>
    /// ID of clicked result (if any).
    /// </summary>
    public string? ClickedResultId { get; set; }

    /// <summary>
    /// Time taken to perform search (milliseconds).
    /// </summary>
    public long ProcessingTimeMs { get; init; }

    /// <summary>
    /// When the search was performed.
    /// </summary>
    public DateTime SearchedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// User's IP address or session for anonymous users.
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Detected language of query.
    /// </summary>
    public string Language { get; init; } = "en";
}
