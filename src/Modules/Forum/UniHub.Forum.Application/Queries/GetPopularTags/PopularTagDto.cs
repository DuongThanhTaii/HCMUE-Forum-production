namespace UniHub.Forum.Application.Queries.GetPopularTags;

public sealed record PopularTagDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public int UsageCount { get; init; }
}
