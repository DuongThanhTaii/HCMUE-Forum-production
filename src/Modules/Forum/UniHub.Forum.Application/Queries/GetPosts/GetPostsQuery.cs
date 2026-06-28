using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Queries.GetPosts;

/// <summary>
/// Query to get a paginated list of posts with optional filtering.
/// SortBy: 0 = newest (default), 1 = top (VoteScore desc)
/// </summary>
public sealed record GetPostsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? CategoryId = null,
    Guid? ThreadChannelId = null,
    int? Type = null,
    int? Status = null,
    int SortBy = 0,
    IReadOnlyList<Guid>? CategoryIds = null) : IQuery<GetPostsResult>;
