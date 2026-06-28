using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Queries.GetPopularTags;

public sealed record GetPopularTagsQuery(int Count = 10) : IQuery<IEnumerable<PopularTagDto>>;
