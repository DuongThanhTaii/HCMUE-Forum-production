using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Queries.GetPostById;

/// <summary>
/// Handler for getting a post by ID
/// </summary>
public sealed class GetPostByIdQueryHandler : IQueryHandler<GetPostByIdQuery, PostDetailResult?>
{
    private readonly IPostRepository _postRepository;
    private readonly IBookmarkRepository _bookmarkRepository;

    public GetPostByIdQueryHandler(
        IPostRepository postRepository,
        IBookmarkRepository bookmarkRepository)
    {
        _postRepository = postRepository;
        _bookmarkRepository = bookmarkRepository;
    }

    public async Task<Result<PostDetailResult?>> Handle(
        GetPostByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (request.PostId == Guid.Empty)
        {
            return Result.Failure<PostDetailResult?>(
                new Error("GetPostById.InvalidId", "Post ID cannot be empty"));
        }

        var postIdVo = new PostId(request.PostId);
        var result = await _postRepository.GetPostDetailsAsync(
            postIdVo,
            cancellationToken);

        if (result is null)
        {
            return Result.Success(result);
        }

        var isBookmarked = false;
        if (request.CurrentUserId is { } userId)
        {
            var bookmark = await _bookmarkRepository.GetByUserAndPostAsync(
                userId,
                postIdVo,
                cancellationToken);
            isBookmarked = bookmark is not null;
        }

        return Result.Success<PostDetailResult?>(result with { IsBookmarkedByCurrentUser = isBookmarked });
    }
}
