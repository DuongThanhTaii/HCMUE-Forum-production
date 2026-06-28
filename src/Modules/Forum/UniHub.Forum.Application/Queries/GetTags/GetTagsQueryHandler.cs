using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Queries.GetTags;

public sealed class GetTagsQueryHandler : IQueryHandler<GetTagsQuery, GetTagsResult>
{
    private readonly ITagRepository _tagRepository;

    public GetTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<GetTagsResult>> Handle(GetTagsQuery query, CancellationToken cancellationToken)
    {
        if (query.PageNumber <= 0)
        {
            return Result.Failure<GetTagsResult>(TagErrors.InvalidPageNumber);
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            return Result.Failure<GetTagsResult>(TagErrors.InvalidPageSize);
        }

        var result = await _tagRepository.GetTagsAsync(
            query.PageNumber,
            query.PageSize,
            query.SearchTerm,
            query.OrderByUsage,
            cancellationToken);

        return Result.Success(result);
    }
}
