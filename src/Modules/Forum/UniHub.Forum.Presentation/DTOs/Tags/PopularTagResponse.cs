namespace UniHub.Forum.Presentation.DTOs.Tags;

/// <summary>
/// Response containing popular tag information
/// </summary>
public sealed record PopularTagResponse
{
    public string Name { get; init; } = string.Empty;
    public int PostCount { get; init; }
    public double PopularityScore { get; init; }
}
