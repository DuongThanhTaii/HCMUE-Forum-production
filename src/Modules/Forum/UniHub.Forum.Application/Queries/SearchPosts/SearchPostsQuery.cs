using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Queries.SearchPosts;

/// <summary>
/// Query to search posts by title, content, and tags
/// </summary>
public sealed record SearchPostsQuery(
    string SearchTerm,
    Guid? CategoryId = null,
    int? PostType = null,
    IEnumerable<string>? Tags = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<SearchPostsResult>;
