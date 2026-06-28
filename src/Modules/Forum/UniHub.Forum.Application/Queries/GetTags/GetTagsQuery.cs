using UniHub.SharedKernel.CQRS;

namespace UniHub.Forum.Application.Queries.GetTags;

public sealed record GetTagsQuery(
    int PageNumber = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    bool OrderByUsage = false) : IQuery<GetTagsResult>;
