using UniHub.Forum.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Queries.GetPosts;

/// <summary>
/// Handler for getting paginated posts with optional filtering
/// </summary>
public sealed class GetPostsQueryHandler : IQueryHandler<GetPostsQuery, GetPostsResult>
{
    private readonly IPostRepository _postRepository;

    public GetPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<GetPostsResult>> Handle(
        GetPostsQuery request,
        CancellationToken cancellationToken)
    {
        // Validate page number
        if (request.PageNumber < 1)
        {
            return Result.Failure<GetPostsResult>(
                new Error("GetPosts.InvalidPageNumber", "Page number must be greater than 0"));
        }

        // Validate page size
        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<GetPostsResult>(
                new Error("GetPosts.InvalidPageSize", "Page size must be between 1 and 100"));
        }

        var result = await _postRepository.GetPostsAsync(
            request.PageNumber,
            request.PageSize,
            request.CategoryId,
            request.ThreadChannelId,
            request.Type,
            request.Status,
            request.SortBy,
            request.CategoryIds,
            cancellationToken);

        return Result.Success(result);
    }
}
