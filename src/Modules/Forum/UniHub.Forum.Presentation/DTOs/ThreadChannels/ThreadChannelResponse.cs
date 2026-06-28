namespace UniHub.Forum.Presentation.DTOs.ThreadChannels;

public sealed record ThreadChannelResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsActive { get; init; }
    public bool AllowPinnedComments { get; init; }
    public bool AllowAcceptedAnswers { get; init; }
    public bool AllowModeratorActions { get; init; }
}
