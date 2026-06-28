using UniHub.Forum.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Queries.GetBookmarkedPosts;

public sealed class GetBookmarkedPostsQueryHandler : IQueryHandler<GetBookmarkedPostsQuery, GetBookmarkedPostsResult>
{
    private readonly IBookmarkRepository _bookmarkRepository;

    public GetBookmarkedPostsQueryHandler(IBookmarkRepository bookmarkRepository)
    {
        _bookmarkRepository = bookmarkRepository;
    }

    public async Task<Result<GetBookmarkedPostsResult>> Handle(GetBookmarkedPostsQuery query, CancellationToken cancellationToken)
    {
        if (query.PageNumber <= 0)
        {
            return Result.Failure<GetBookmarkedPostsResult>(new Error(
                "GetBookmarkedPosts.InvalidPageNumber",
                "Page number must be greater than 0"));
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            return Result.Failure<GetBookmarkedPostsResult>(new Error(
                "GetBookmarkedPosts.InvalidPageSize",
                "Page size must be between 1 and 100"));
        }

        var result = await _bookmarkRepository.GetBookmarkedPostsAsync(
            query.UserId,
            query.PageNumber,
            query.PageSize,
            cancellationToken);

        return Result.Success(result);
    }
}
