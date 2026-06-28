using UniHub.Forum.Application.Abstractions;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Queries.SearchPosts;

/// <summary>
/// Handler for searching posts using full-text search
/// </summary>
public sealed class SearchPostsQueryHandler : IQueryHandler<SearchPostsQuery, SearchPostsResult>
{
    private readonly IPostRepository _postRepository;

    public SearchPostsQueryHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<SearchPostsResult>> Handle(
        SearchPostsQuery request, 
        CancellationToken cancellationToken)
    {
        // Validate search term
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return Result.Failure<SearchPostsResult>(
                new Error("Search.EmptySearchTerm", "Search term cannot be empty"));
        }

        // Validate page number
        if (request.PageNumber < 1)
        {
            return Result.Failure<SearchPostsResult>(
                new Error("Search.InvalidPageNumber", "Page number must be greater than 0"));
        }

        // Validate page size
        if (request.PageSize < 1 || request.PageSize > 100)
        {
            return Result.Failure<SearchPostsResult>(
                new Error("Search.InvalidPageSize", "Page size must be between 1 and 100"));
        }

        // Execute search via repository
        var searchResult = await _postRepository.SearchAsync(
            request.SearchTerm,
            request.CategoryId,
            request.PostType,
            request.Tags,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        return Result.Success(searchResult);
    }
}
