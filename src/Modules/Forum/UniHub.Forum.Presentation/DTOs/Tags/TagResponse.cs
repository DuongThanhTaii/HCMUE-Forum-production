namespace UniHub.Forum.Presentation.DTOs.Tags;

/// <summary>
/// Response containing tag information
/// </summary>
public sealed record TagResponse
{
    public string Name { get; init; } = string.Empty;
    public int PostCount { get; init; }
}
