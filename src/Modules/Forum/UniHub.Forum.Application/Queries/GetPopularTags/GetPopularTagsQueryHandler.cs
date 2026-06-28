using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Application.Queries.GetPopularTags;

public sealed class GetPopularTagsQueryHandler : IQueryHandler<GetPopularTagsQuery, IEnumerable<PopularTagDto>>
{
    private readonly ITagRepository _tagRepository;

    public GetPopularTagsQueryHandler(ITagRepository tagRepository)
    {
        _tagRepository = tagRepository;
    }

    public async Task<Result<IEnumerable<PopularTagDto>>> Handle(GetPopularTagsQuery query, CancellationToken cancellationToken)
    {
        if (query.Count < 1 || query.Count > 50)
        {
            return Result.Failure<IEnumerable<PopularTagDto>>(TagErrors.InvalidCount);
        }

        var tags = await _tagRepository.GetPopularTagsAsync(query.Count, cancellationToken);

        return Result.Success(tags);
    }
}
