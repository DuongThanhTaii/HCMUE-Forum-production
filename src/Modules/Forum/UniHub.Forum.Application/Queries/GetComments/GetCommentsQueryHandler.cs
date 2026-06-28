using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Domain.Posts;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Queries.GetComments;

/// <summary>
/// Handler for getting paginated comments for a post
/// </summary>
public sealed class GetCommentsQueryHandler : IQueryHandler<GetCommentsQuery, GetCommentsResult>
{
    private readonly ICommentRepository _commentRepository;

    public GetCommentsQueryHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<GetCommentsResult>> Handle(
        GetCommentsQuery request,
        CancellationToken cancellationToken)
    {
        // Validate post ID
        if (request.PostId == Guid.Empty)
        {
            return Result.Failure<GetCommentsResult>(
                new Error("GetComments.InvalidPostId", "Post ID cannot be empty"));
        }

        // Validate page number
        if (request.PageNumber < 1)
        {
            return Result.Failure<GetCommentsResult>(
                new Error("GetComments.InvalidPageNumber", "Page number must be greater than 0"));
        }

        // Validate page size
        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<GetCommentsResult>(
                new Error("GetComments.InvalidPageSize", "Page size must be between 1 and 100"));
        }

        var result = await _commentRepository.GetCommentsByPostIdAsync(
            new PostId(request.PostId),
            request.CurrentUserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(result);
    }
}
