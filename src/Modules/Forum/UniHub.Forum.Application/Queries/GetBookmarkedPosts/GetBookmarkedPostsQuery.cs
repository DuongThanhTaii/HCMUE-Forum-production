using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Queries.GetBookmarkedPosts;

public sealed record GetBookmarkedPostsQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<GetBookmarkedPostsResult>;
