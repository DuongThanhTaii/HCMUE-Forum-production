namespace UniHub.SharedKernel.Persistence;

/// <summary>
/// Generic paged result for paginated queries.
/// Use this class for new queries that return paginated lists.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
public sealed record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Creates a new <see cref="PagedResult{T}"/> instance.
    /// </summary>
    public PagedResult(IReadOnlyList<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Creates an empty paged result.
    /// </summary>
    public static PagedResult<T> Empty(int pageNumber = 1, int pageSize = 20)
        => new(Array.Empty<T>(), 0, pageNumber, pageSize);
}
