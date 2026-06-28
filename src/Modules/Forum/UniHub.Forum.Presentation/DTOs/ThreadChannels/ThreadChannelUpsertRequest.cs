namespace UniHub.Forum.Presentation.DTOs.ThreadChannels;

public sealed record ThreadChannelUpsertRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public bool AllowPinnedComments { get; init; } = true;
    public bool AllowAcceptedAnswers { get; init; } = true;
    public bool AllowModeratorActions { get; init; } = true;
}
